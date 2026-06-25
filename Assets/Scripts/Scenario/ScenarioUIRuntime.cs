using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioUIRuntime
{
    private readonly Canvas rootCanvas;
    private readonly RectTransform panelRoot;
    private readonly TextMeshProUGUI headerText;
    private readonly TextMeshProUGUI bodyText;
    private readonly TextMeshProUGUI infoText;
    private readonly TextMeshProUGUI scoreText;
    private readonly TextMeshProUGUI timerText;
    private readonly RectTransform optionsContainer;
    private readonly Button primaryButton;

    private readonly List<Button> optionButtons = new List<Button>();

    private Action primaryButtonAction;

    public ScenarioUIRuntime(Canvas canvas)
    {
        rootCanvas = canvas;
        panelRoot = BuildPanel(canvas);

        var headerRow = CreateHorizontalStack(panelRoot, "ScenarioHeaderRow", 12f, TextAnchor.UpperLeft);
        headerText = CreateText(headerRow, "ScenarioHeader", 26f, FontStyles.Bold, new Color(0.78f, 0.93f, 1f), TextAlignmentOptions.MidlineLeft, true, 44f, 1f);
        headerText.enableAutoSizing = true;
        headerText.fontSizeMin = 20f;
        headerText.fontSizeMax = 30f;

        var metaColumn = CreateVerticalStack(headerRow, "ScenarioMeta", 4f, false, TextAnchor.UpperRight, 0f, 150f);
        scoreText = CreateText(metaColumn, "ScenarioScore", 18f, FontStyles.Bold, new Color(0.85f, 0.92f, 1f), TextAlignmentOptions.MidlineRight, false, 28f, 0f);
        scoreText.text = "Score: --";
        timerText = CreateText(metaColumn, "ScenarioTimer", 16f, FontStyles.Bold, new Color(1f, 0.82f, 0.58f), TextAlignmentOptions.MidlineRight, false, 24f, 0f);
        timerText.gameObject.SetActive(false);

        CreateDivider(panelRoot);

        bodyText = CreateText(panelRoot, "ScenarioBody", 20f, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft, true, 96f, 1f);
        bodyText.lineSpacing = 4f;
        infoText = CreateText(panelRoot, "ScenarioInfo", 16f, FontStyles.Italic, new Color(0.82f, 0.84f, 0.94f), TextAlignmentOptions.TopLeft, true, 44f, 1f);
        infoText.margin = new Vector4(0f, 6f, 0f, 0f);
        infoText.gameObject.SetActive(false);

        optionsContainer = CreateVerticalStack(panelRoot, "ScenarioOptions", 10f, false, TextAnchor.UpperLeft, 1f, 0f);
        primaryButton = CreateButton(panelRoot, "PrimaryAction", 0.55f, new Color(0.16f, 0.59f, 0.7f, 0.95f), TextAlignmentOptions.Midline);
        optionsContainer.gameObject.SetActive(false);
        primaryButton.gameObject.SetActive(false);

        Hide();
    }

    public void Show()
    {
        panelRoot.gameObject.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.gameObject.SetActive(false);
    }

    public void SetHeader(string text)
    {
        headerText.text = text ?? string.Empty;
    }

    public void SetBody(string text)
    {
        bodyText.text = text ?? string.Empty;
    }

    public void SetInfo(string text)
    {
        infoText.text = text ?? string.Empty;
        infoText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    public void SetScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void SetTimer(string text)
    {
        timerText.text = text ?? string.Empty;
        timerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    public void ClearOptions()
    {
        if (optionsContainer == null)
        {
            optionButtons.Clear();
            return;
        }

        foreach (var button in optionButtons)
        {
            if (button != null)
            {
                UnityEngine.Object.Destroy(button.gameObject);
            }
        }

        optionButtons.Clear();

        optionsContainer.gameObject.SetActive(false);
    }

    public void ShowOptions(IReadOnlyList<OptionViewData> options)
    {
        ClearOptions();
        primaryButton.gameObject.SetActive(false);

        if (options == null || options.Count == 0)
        {
            optionsContainer.gameObject.SetActive(false);
            return;
        }

        optionsContainer.gameObject.SetActive(true);

        foreach (var option in options)
        {
            var button = CreateButton(optionsContainer, option.Label, 0.25f, option.Color ?? new Color(0.27f, 0.46f, 0.86f), TextAlignmentOptions.MidlineLeft);
            button.onClick.AddListener(() => option.Callback?.Invoke());
            optionButtons.Add(button);
        }
    }

    public void ShowPrimary(string label, Action action)
    {
        optionsContainer.gameObject.SetActive(false);
        ClearOptions();

        primaryButton.gameObject.SetActive(true);
        ConfigurePrimary(action);
        UpdateButtonLabel(primaryButton, label, TextAlignmentOptions.Midline);
    }

    public void SetPrimaryInteractable(bool interactable)
    {
        primaryButton.interactable = interactable;
    }

    private RectTransform BuildPanel(Canvas canvas)
    {
        var panelGO = new GameObject("ScenarioPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
        var rect = panelGO.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(36f, -36f);
        rect.sizeDelta = new Vector2(380f, 0f);

        var image = panelGO.GetComponent<Image>();
        image.color = new Color(0.06f, 0.09f, 0.16f, 0.88f);

        var layout = panelGO.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 24, 26);
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childAlignment = TextAnchor.UpperLeft;

        var fitter = panelGO.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rect;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, bool allowWrap, float minHeight, float flexibleWidth)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
#pragma warning disable CS0618
        text.enableWordWrapping = allowWrap;
#pragma warning restore CS0618
        text.alignment = alignment;
        text.richText = true;
        text.margin = new Vector4(0, 0, 0, 0);
        text.text = string.Empty;

        var layout = go.AddComponent<LayoutElement>();
        layout.minHeight = minHeight > 0f ? minHeight : fontSize * 1.3f;
        layout.preferredHeight = layout.minHeight;
        layout.flexibleWidth = Mathf.Max(0f, flexibleWidth);

        return text;
    }

    private RectTransform CreateVerticalStack(Transform parent, string name, float spacing, bool flexibleHeight, TextAnchor alignment, float flexibleWidth, float minWidth)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        var layoutGroup = go.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = spacing;
        layoutGroup.childAlignment = alignment;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        var element = go.AddComponent<LayoutElement>();
        element.flexibleWidth = Mathf.Max(0f, flexibleWidth);
        element.minWidth = Mathf.Max(0f, minWidth);
        if (minWidth > 0f)
        {
            element.preferredWidth = minWidth;
        }
        if (flexibleHeight)
        {
            element.flexibleHeight = 1f;
        }

        return rect;
    }

    private RectTransform CreateHorizontalStack(Transform parent, string name, float spacing, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        var layoutGroup = go.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = spacing;
        layoutGroup.childAlignment = alignment;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        var element = go.AddComponent<LayoutElement>();
        element.flexibleWidth = 1f;

        return rect;
    }

    private void CreateDivider(Transform parent)
    {
        var go = new GameObject("ScenarioDivider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(0f, 2f);

        var image = go.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.1f);

        var layout = go.AddComponent<LayoutElement>();
        layout.minHeight = 2f;
        layout.preferredHeight = 2f;
        layout.flexibleWidth = 1f;
    }

    private Button CreateButton(Transform parent, string label, float heightPercent, Color? backgroundColor = null, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0f, Mathf.Lerp(40f, 72f, Mathf.Clamp01(heightPercent)));

        var image = go.GetComponent<Image>();
        image.color = backgroundColor ?? new Color(0.18f, 0.32f, 0.58f, 0.95f);
        image.raycastTarget = true;

        var button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.colorMultiplier = 1f;
        colors.normalColor = image.color;
        colors.highlightedColor = Color.Lerp(image.color, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(image.color, Color.black, 0.25f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        button.colors = colors;

        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.minHeight = Mathf.Lerp(48f, 72f, Mathf.Clamp01(heightPercent));
        layoutElement.preferredHeight = layoutElement.minHeight;
        layoutElement.flexibleWidth = 1f;
        layoutElement.minWidth = 0f;

        UpdateButtonLabel(button, label, alignment);

        return button;
    }

    private void UpdateButtonLabel(Button button, string text, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        TextMeshProUGUI tmp;
        var existing = button.GetComponentInChildren<TextMeshProUGUI>();
        if (existing != null && existing.transform.parent == button.transform)
        {
            tmp = existing;
        }
        else
        {
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(button.transform, false);
            var rect = labelGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(18f, 6f);
            rect.offsetMax = new Vector2(-18f, -6f);

            tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.font = TMP_Settings.defaultFontAsset;
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
#pragma warning disable CS0618
            tmp.enableWordWrapping = false;
#pragma warning restore CS0618
            tmp.alignment = alignment;
            tmp.color = Color.white;
        }

        tmp.alignment = alignment;
        tmp.text = text?.Trim() ?? string.Empty;
    }

    public void ConfigurePrimary(Action action)
    {
        primaryButtonAction = action;
        primaryButton.onClick.RemoveAllListeners();
        if (action != null)
        {
            primaryButton.onClick.AddListener(() => action.Invoke());
        }
    }

    public void SetPrimaryLabel(string label)
    {
        UpdateButtonLabel(primaryButton, label, TextAlignmentOptions.Midline);
    }

    public void TriggerPrimary()
    {
        primaryButtonAction?.Invoke();
    }

    public bool HasPrimaryAction => primaryButton != null && primaryButton.gameObject.activeInHierarchy && primaryButton.interactable;

    public struct OptionViewData
    {
        public string Label;
        public Action Callback;
        public Color? Color;
    }
}
