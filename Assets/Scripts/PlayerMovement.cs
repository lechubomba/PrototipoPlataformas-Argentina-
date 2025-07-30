using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadMovimiento = 5f;
    public float fuerzaSalto = 7f;
    [Tooltip("Multiplicador de fuerza de salto al tener JumpBoost")] public float jumpBoostMultiplier = 1.25f;
    public float velocidadDash = 15f;
    public float duracionDash = 0.2f;
    [Tooltip("Cooldown entre dashes en segundos")] public float dashCooldown = 1f;
    private float nextDashTime;
    public float fuerzaWallJump = 7f;
    public float velocidadWallSlide = 2f;

    [Header("Ground Pound")]
    [Tooltip("Velocidad de caída rápida en GroundPound")] public float velocidadGroundPound = 20f;
    [Tooltip("Tiempo de recuperación tras GroundPound")] public float groundPoundRecovery = 0.2f;

    [Header("Wall Jump Settings")]
    public float wallStickTime = 0.2f;
    public float wallCoyoteTime = 0.2f;

    [Header("Chequeo de Suelo y Pared")]
    public Transform chequeoSuelo;
    public float radioChequeoSuelo = 0.2f;
    public LayerMask capaSuelo;
    public Transform chequeoParedIzquierda;
    public Transform chequeoParedDerecha;
    public float distanciaChequeoPared = 0.5f;
    public LayerMask capaPared;

    // Estado interno
    private Rigidbody rb;
    private float entradaMovimiento;
    private bool enSuelo;
    private bool tocandoPared;
    private bool paredDerecha;
    private Vector3 normalPared;
    private bool puedeDobleSalto;
    private bool estaDash;
    private float temporizadorDash;
    private float wallStickCounter;
    private float wallCoyoteCounter;
    private bool isGroundPound;
    private float groundPoundTimer;

    // Animaciones
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        anim = GetComponentInChildren<Animator>();
        if (anim == null) Debug.LogWarning("No se encontró Animator en hijos.");

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogWarning("No se encontró SpriteRenderer en hijos.");
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 0) Si estás en dash, gestiona recuperación y sal de aquí
        if (estaDash)
        {
            temporizadorDash -= dt;
            if (temporizadorDash <= 0f) TerminarDash();
            return;
        }

        // 1) Movimiento horizontal
        entradaMovimiento = Input.GetAxisRaw("Horizontal");
        Vector3 v = rb.velocity;
        v.x = entradaMovimiento * velocidadMovimiento;
        rb.velocity = v;

        // 2) Flip del sprite
        if (spriteRenderer != null && entradaMovimiento != 0f)
            spriteRenderer.flipX = entradaMovimiento < 0f;

        // 3) Chequeo de suelo y doble salto
        bool wasGrounded = enSuelo;
        enSuelo = Physics.CheckSphere(chequeoSuelo.position, radioChequeoSuelo, capaSuelo);
        if (enSuelo && !wasGrounded)
        {
            // Aterrizaje: dispara Land y resetea salto
            if (anim != null)
            {
                anim.SetBool("isJumping", false);
                // Forzar retorno a Move (Idle/Run Blend Tree)
                anim.CrossFade("Move", 0f, 0);
            }

            // Reset de doble salto
            puedeDobleSalto = GameManager.Instance.HasAbility("DoubleJump");
        }
        // 4) Actualizar parámetros del Animator
        if (anim != null)
        {
            anim.SetBool("isGrounded", enSuelo);
            anim.SetFloat("Speed", enSuelo ? Mathf.Abs(entradaMovimiento) : 0f);
            anim.SetFloat("VerticalSpeed", rb.velocity.y);
        }

        // 5) Salto / Doble salto
        if (Input.GetButtonDown("Jump"))
        {
            if (enSuelo)
            {
                Saltar();
                if (anim != null) anim.SetTrigger("DoJump");    // usa un Trigger genérico "DoJump"
            }
            else if (puedeDobleSalto)
            {
                Saltar();
                puedeDobleSalto = false;
                if (anim != null) anim.SetTrigger("DoDoubleJump");
            }
        }

        // 6) GroundPound
        if (!enSuelo
           && Input.GetKeyDown(KeyCode.S)
           && GameManager.Instance.HasAbility("GroundPound")
           && !isGroundPound)
        {
            // 1) Disparamos la animación
            anim.SetTrigger("DoGroundPound");

            // 2) Ejecutamos la física de caída rápida
            IniciarGroundPound();
        }

        // 7) Dash
        if (Time.time >= nextDashTime
            && Input.GetButtonDown("Fire3")
            && Mathf.Abs(entradaMovimiento) > 0f
            && GameManager.Instance.HasAbility("Dash"))
        {
            // 1) Disparamos la animación Dash
            if (anim != null)
                Debug.Log("[Dash] Trigger disparado");
            anim.SetTrigger("DoDash");

            // 2) Física del dash
            IniciarDash(new Vector3(entradaMovimiento, 0f, 0f));
            nextDashTime = Time.time + dashCooldown;
        }

        // 8) Recuperación GroundPound
        if (isGroundPound)
        {
            groundPoundTimer -= dt;
            if (groundPoundTimer <= 0f) isGroundPound = false;
        }

        // 9) Buffers wall‑jump
        if (tocandoPared) wallCoyoteCounter = wallCoyoteTime;
        else wallCoyoteCounter -= dt;
        if (tocandoPared && !enSuelo) wallStickCounter = wallStickTime;
        else wallStickCounter -= dt;

        // 10) Chequeo de pared
        RaycastHit hit;
        paredDerecha = Physics.Raycast(chequeoParedDerecha.position, transform.right, out hit, distanciaChequeoPared, capaPared);
        bool paredIzquierda = Physics.Raycast(chequeoParedIzquierda.position, -transform.right, out hit, distanciaChequeoPared, capaPared);
        tocandoPared = paredDerecha || paredIzquierda;
        normalPared = paredDerecha ? -transform.right : (paredIzquierda ? transform.right : Vector3.zero);

        // 11) Wall‑jump
        if (Input.GetButtonDown("Jump")
            && (tocandoPared || wallCoyoteCounter > 0f)
            && GameManager.Instance.HasAbility("WallJump"))
        {
            SaltoDesdePared();
            if (anim != null) anim.SetTrigger("DoJump");
        }
    }

    void FixedUpdate()
    {
        if (estaDash || isGroundPound) return;

        // Wall slide
        if (tocandoPared && !enSuelo)
        {
            if (wallStickCounter > 0f)
            {
                rb.velocity = new Vector3(0f, -velocidadWallSlide, 0f);
                return;
            }
            else
            {
                float dirX = paredDerecha ? Mathf.Min(entradaMovimiento, 0f) : Mathf.Max(entradaMovimiento, 0f);
                rb.velocity = new Vector3(dirX * velocidadMovimiento, -velocidadWallSlide, 0f);
                return;
            }
        }

        // Movimiento normal
        rb.velocity = new Vector3(entradaMovimiento * velocidadMovimiento, rb.velocity.y, 0f);
    }

    void Saltar()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
        float fuerza = fuerzaSalto;
        if (GameManager.Instance.HasAbility("JumpBoost")) fuerza *= jumpBoostMultiplier;
        rb.AddForce(Vector3.up * fuerza, ForceMode.Impulse);
    }

    void SaltoDesdePared()
    {
        Vector3 dir = (Vector3.up * 0.7f + normalPared * 0.3f).normalized;
        rb.velocity = Vector3.zero;
        float fuerza = fuerzaWallJump;
        if (GameManager.Instance.HasAbility("JumpBoost")) fuerza *= jumpBoostMultiplier;
        rb.AddForce(dir * fuerza, ForceMode.Impulse);
        wallStickCounter = 0f;
        wallCoyoteCounter = 0f;
    }

    void IniciarDash(Vector3 dir)
    {
        estaDash = true;
        temporizadorDash = duracionDash;
        rb.velocity = dir * velocidadDash;
        rb.useGravity = false;
    }

    void TerminarDash()
    {
        estaDash = false;
        rb.useGravity = true;
    }

    void IniciarGroundPound()
    {
        isGroundPound = true;
        groundPoundTimer = groundPoundRecovery;
        rb.velocity = Vector3.down * velocidadGroundPound;
    }

    void OnCollisionEnter(Collision col)
    {
        if (isGroundPound && col.contacts[0].normal.y > 0.5f)
        {
            var weak = col.collider.GetComponent<WeakPlatform>();
            if (weak) weak.Break();
            isGroundPound = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (chequeoSuelo) Gizmos.DrawWireSphere(chequeoSuelo.position, radioChequeoSuelo);
        if (chequeoParedIzquierda && chequeoParedDerecha)
        {
            Gizmos.DrawLine(chequeoParedDerecha.position,
                chequeoParedDerecha.position + transform.right * distanciaChequeoPared);
            Gizmos.DrawLine(chequeoParedIzquierda.position,
                chequeoParedIzquierda.position - transform.right * distanciaChequeoPared);
        }
    }
}