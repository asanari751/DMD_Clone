using UnityEngine;
using TMPro;
using System.Collections;

public class DamageIndicator : MonoBehaviour
{
    [Header("Damage Indicator")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform damageTextParent;

    [Header("Properties")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector3 damageOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 healOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private float randomOffsetRange = 1f;

    [Header("Curve")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.5f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private Vector2 moveDirection = new Vector2(1f, 1f);

    public static DamageIndicator Instance { get; private set; }
    private bool isEnabled = true;

    private void Awake()
    {
        InitializeSingleton();
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
        }

        isEnabled = PlayerPrefs.GetInt("ShowDamageIndicator", 1) == 1;
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;

            if (transform.parent != null)
            {
                GameObject root = new GameObject("DamageIndicatorRoot");
                transform.SetParent(root.transform);
                DontDestroyOnLoad(root);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public void ShowDamage(Vector3 position, int amount)
    {
        if (!isEnabled) return;

        GameObject damageText = CreateDamageText(position);
        SetDamageTextProperties(damageText, amount);
        StartCoroutine(AnimateText(damageText));
    }

    private GameObject CreateDamageText(Vector3 position)
    {
        GameObject damageText = Instantiate(textPrefab, damageTextParent != null ? damageTextParent : canvas.transform);
        RectTransform rectTransform = damageText.GetComponent<RectTransform>();

        Vector2 randomOffset = new Vector2(
            Random.Range(-randomOffsetRange, randomOffsetRange),
            Random.Range(-randomOffsetRange, randomOffsetRange)
        );

        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(position + damageOffset);
        Vector2 worldObjectScreenPosition = new Vector2(
            ((viewportPosition.x * canvas.pixelRect.width) - (canvas.pixelRect.width * 0.5f)),
            ((viewportPosition.y * canvas.pixelRect.height) - (canvas.pixelRect.height * 0.5f)));

        rectTransform.anchoredPosition = worldObjectScreenPosition + randomOffset;
        return damageText;
    }

    private void SetDamageTextProperties(GameObject damageText, int amount)
    {
        TextMeshProUGUI textMesh = damageText.GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = amount.ToString();
            textMesh.fontSize = fontSize;
        }
        else
        {
            Destroy(damageText);
        }
    }

    private IEnumerator AnimateText(GameObject damageText)
    {
        if (damageText == null) yield break;

        TextMeshProUGUI textMesh = damageText.GetComponent<TextMeshProUGUI>();
        float elapsedTime = 0f;
        Vector3 startPosition = damageText.transform.position;
        Vector3 startScale = damageText.transform.localScale;
        Color startColor = textMesh.color;

        while (elapsedTime < lifetime && damageText != null)
        {
            UpdateTextAnimation(damageText, textMesh, startPosition, startScale, startColor, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (damageText != null)
            Destroy(damageText);
    }


    private void UpdateTextAnimation(GameObject damageText, TextMeshProUGUI textMesh, Vector3 startPosition, Vector3 startScale, Color startColor, float elapsedTime)
    {
        if (damageText == null) return;

        float t = elapsedTime / lifetime;
        float scale = Mathf.Lerp(1f, minScale, scaleCurve.Evaluate(t));
        float alpha = Mathf.Lerp(1f, minAlpha, alphaCurve.Evaluate(t));

        Vector3 normalizedDirection = moveDirection.normalized;
        damageText.transform.position = startPosition + (Vector3)(normalizedDirection * moveSpeed * elapsedTime);
        damageText.transform.localScale = startScale * scale;
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
    }

    // PlayerSkills B7, B8

    public void ShowHeal(Vector3 position, int amount)
    {
        GameObject healText = CreateDamageText(position + healOffset);
        TextMeshProUGUI textMesh = healText.GetComponent<TextMeshProUGUI>();
        textMesh.text = amount.ToString();
        textMesh.color = Color.green;
        StartCoroutine(AnimateText(healText));
    }
}