using UnityEngine;
using System.Collections;

public class SerpentHead : MonoBehaviour
{
    public enum SerpentState
    {
        Idle,
        BeginLaserCharge,
        LaserFire,
        LaserEnd,
        TailAttack,
        SpawnSouls,
        RisingSouls,
        ScrewDrop
    }

    [Header("Defeat Sequence")]
    public CanvasGroup fadeScreen;
    public CanvasGroup endMessage;
    public float fadeSpeed = 1f;
    public float messageDelay = 1.5f;

    [Header("References")]
    public Animator anim;
    public SerpentChargeController chargeController;
    public LaserSegmentController laser1;

    [Header("Tail Attack")]
    public GameObject tailObject;
    public Transform leftTailPoint;
    public Transform rightTailPoint;
    public GameObject tailWarningZone;
    public float tailWindup = 0.8f;
    public float tailSwipeTime = 0.4f;
    public float tailFollowThrough = 0.6f;
    protected bool tailAttackRunning = false;

    [Header("Rising Souls")]
    public Transform topLeft;
    public Transform topRight;
    public Transform bottomLeft;
    public Transform bottomRight;

    public GameObject risingSoulPrefab;
    public int risingSoulCount = 50;
    public float risingSoulDelay = 0.3f;
    public Transform player;


    [Header("Soul Spawn Settings")]
    public Transform[] soulSpawnPoints;
    public GameObject forbiddenSoulPrefab;
    public float soulSpawnDelay = 1.2f;

    [Header("Audio")]
    public AudioClip roarSFX;
    protected AudioSource audioSource;

    [Header("Timings")]
    public float idleDuration = 1.5f;
    public float beginChargeDuration = 1.0f;
    public float fireDuration = 1.0f;
    public float endDuration = 0.8f;

    [Header("Screw Phase")]
    public ScrewController screw;          // persistent screw object
    public float screwDuration = 3.0f;
    protected bool screwStuck = false;
    protected bool screwGone = false;
    protected bool screwDestroyed = false;

    protected SerpentState state = SerpentState.Idle;
    protected float stateTimer = 0f;

    protected int nextAttackIndex = 0;

    protected SerpentState[] attackCycle =
    {
        SerpentState.BeginLaserCharge,
        SerpentState.TailAttack,
        SerpentState.SpawnSouls,
        SerpentState.RisingSouls,
        SerpentState.ScrewDrop
    };

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        foreach (var sr in tailObject.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;

        EnterState(SerpentState.Idle);

        chargeController.OnFire = () => { laser1.Fire(); };
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case SerpentState.Idle:
                UpdateIdle();
                break;

            case SerpentState.BeginLaserCharge:
                UpdateBeginLaserCharge();
                break;

            case SerpentState.LaserFire:
                UpdateLaserFire();
                break;

            case SerpentState.LaserEnd:
                UpdateLaserEnd();
                break;

            case SerpentState.SpawnSouls:
                UpdateSpawnSouls();
                break;

            case SerpentState.TailAttack:
                if (!tailAttackRunning)
                {
                    tailAttackRunning = true;
                    StartCoroutine(DoTailAttack());
                }
                break;

            case SerpentState.ScrewDrop:
                break;
        }
    }

    IEnumerator DoScrewPhase()
    {
        if (screwDestroyed)
        {
            EnterState(SerpentState.Idle);
            yield break;
        }
        screwStuck = false;
        screwGone = false;

        screw.Initialize();
        screw.BeginFall();

        yield return new WaitUntil(() => screwStuck);

        yield return new WaitForSeconds(screwDuration);

        screw.BeginFadeOut();

        yield return new WaitUntil(() => screwGone);

        EnterState(SerpentState.Idle);
    }

    public void OnScrewDestroyed()
    {
        if (screwDestroyed) return;

        screwDestroyed = true;

        StopAllCoroutines();
        state = SerpentState.Idle;

        StartCoroutine(DoDefeatSequence());
    }


    public void OnScrewStuck()
    {
        screwStuck = true;
    }

    public void OnScrewGone()
    {
        screwGone = true;
    }

    protected IEnumerator DoTailAttack()
    {
        Transform point = leftTailPoint;

        tailObject.GetComponent<TailController>().ResetTail();

        tailWarningZone.SetActive(true);
        yield return new WaitForSeconds(tailWindup);

        tailObject.GetComponent<TailController>().BeginWhip(point.position);

        yield return new WaitForSeconds(tailSwipeTime + tailFollowThrough);

        tailWarningZone.SetActive(false);

        tailAttackRunning = false;
        EnterState(SerpentState.Idle);
    }

    protected IEnumerator DoDefeatSequence()
    {
        yield return new WaitForSeconds(2f);

        while (fadeScreen.alpha < 1f)
        {
            fadeScreen.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        DisableEverything();

        yield return new WaitForSeconds(messageDelay);

        while (endMessage.alpha < 1f)
        {
            endMessage.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }
    }

    void DisableEverything()
    {
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour mb in allBehaviours)
        {
            if (mb == this) continue;

            if (mb.gameObject == fadeScreen.gameObject) continue;

            if (mb.gameObject == endMessage.gameObject) continue;

            mb.enabled = false;
        }

        Rigidbody2D[] bodies = FindObjectsOfType<Rigidbody2D>();
        foreach (Rigidbody2D rb in bodies) rb.simulated = false;
    }


    protected IEnumerator DoRisingSouls()
    {
        for (int i = 0; i < risingSoulCount; i++)
        {

            Vector2 randomPos = GetRandomPointInBox();
            Instantiate(risingSoulPrefab, randomPos, Quaternion.identity);

            Vector2 playerPos = player.position;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Vector2 predictedPos = playerPos;

            if (rb != null) predictedPos += rb.linearVelocity * 0.35f;
            predictedPos = ClampToBox(predictedPos);

            Instantiate(risingSoulPrefab, predictedPos, Quaternion.identity);

            yield return new WaitForSeconds(risingSoulDelay);
        }

        EnterState(SerpentState.Idle);
    }


    protected IEnumerator SyncRoarLength()
    {
        yield return null;
        float roarLength = anim.GetCurrentAnimatorStateInfo(0).length;
        soulSpawnDelay = roarLength;
    }


    void EnterState(SerpentState newState)
    {
        state = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case SerpentState.Idle:
                anim.Play("SerpentIdle", 0, 0f);
                break;

            case SerpentState.BeginLaserCharge:
                anim.Play("SerpentBeginLazerCharge", 0, 0f);
                break;

            case SerpentState.LaserFire:
                anim.Play("SerpentLazerChargeAndFire", 0, 0f);
                chargeController.StartCharge();
                break;

            case SerpentState.LaserEnd:
                anim.Play("SerpentLazerEnd", 0, 0f);
                laser1.StopLaser();
                break;

            case SerpentState.SpawnSouls:
                anim.Play("SerpentRoar", 0, 0f);
                if (roarSFX != null) audioSource.PlayOneShot(roarSFX);
                SpawnSoulsNow();
                StartCoroutine(SyncRoarLength());
                break;

            case SerpentState.ScrewDrop:
                StartCoroutine(DoScrewPhase());
                break;
            case SerpentState.RisingSouls:
                StartCoroutine(DoRisingSouls());
                break;

        }
    }

    void UpdateIdle()
    {
        if (screwDestroyed) return;

        if (stateTimer >= idleDuration) EnterState(GetNextState());
    }

    void UpdateBeginLaserCharge()
    {
        if (stateTimer >= beginChargeDuration) EnterState(SerpentState.LaserFire);
    }

    void UpdateLaserFire()
    {
        if (stateTimer >= fireDuration) EnterState(SerpentState.LaserEnd);
    }

    void UpdateLaserEnd()
    {
        if (stateTimer >= endDuration) EnterState(SerpentState.Idle);
    }

    void UpdateSpawnSouls()
    {
        if (stateTimer >= soulSpawnDelay) EnterState(SerpentState.Idle);
    }

    SerpentState GetNextState()
    {
        SerpentState next = attackCycle[nextAttackIndex];
        nextAttackIndex = (nextAttackIndex + 1) % attackCycle.Length;
        return next;
    }

    void SpawnSoulsNow()
    {
        if (forbiddenSoulPrefab == null || soulSpawnPoints == null) return;

        foreach (Transform t in soulSpawnPoints)
        {
            if (t != null) Instantiate(forbiddenSoulPrefab, t.position, Quaternion.identity);
        }
    }
    Vector2 GetRandomPointInBox()
    {
        float x = Random.Range(bottomLeft.position.x, bottomRight.position.x);
        float y = Random.Range(bottomLeft.position.y, topLeft.position.y);
        return new Vector2(x, y);
    }
    Vector2 ClampToBox(Vector2 pos)
    {
        float x = Mathf.Clamp(pos.x, bottomLeft.position.x, bottomRight.position.x);
        float y = Mathf.Clamp(pos.y, bottomLeft.position.y, topLeft.position.y);
        return new Vector2(x, y);
    }
}
