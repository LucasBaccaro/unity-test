using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Herramienta para crear la UI básica de combate automáticamente.
/// </summary>
public class SetupUIScene
{
    [MenuItem("Tools/Create Combat UI")]
    public static void CreateUI()
    {
        // 1. Crear Canvas si no existe
        GameObject canvasVal = GameObject.Find("Canvas");
        if (canvasVal == null)
        {
            canvasVal = new GameObject("Canvas");
            Canvas c = canvasVal.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasVal.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasVal.AddComponent<GraphicRaycaster>();
        }

        // 2. Crear PlayerHUD
        GameObject hudObj = new GameObject("PlayerHUD");
        hudObj.transform.SetParent(canvasVal.transform, false);
        RectTransform hudRect = hudObj.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(0, 1);
        hudRect.pivot = new Vector2(0, 1);
        hudRect.anchoredPosition = new Vector2(20, -20);

        PlayerHUD hudScript = hudObj.AddComponent<PlayerHUD>();

        // Health Bar
        GameObject healthObj = CreateSlider("HealthBar", Color.red, hudObj.transform, new Vector2(0, 0));
        hudScript.GetType().GetField("healthSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(hudScript, healthObj.GetComponent<Slider>());
        
        GameObject healthTextObj = CreateText("HealthText", "100/100", healthObj.transform);
        hudScript.GetType().GetField("healthText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(hudScript, healthTextObj.GetComponent<TextMeshProUGUI>());

        // Mana Bar
        GameObject manaObj = CreateSlider("ManaBar", Color.blue, hudObj.transform, new Vector2(0, -40));
        hudScript.GetType().GetField("manaSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(hudScript, manaObj.GetComponent<Slider>());
        
        GameObject manaTextObj = CreateText("ManaText", "100/100", manaObj.transform);
        hudScript.GetType().GetField("manaText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(hudScript, manaTextObj.GetComponent<TextMeshProUGUI>());

        // 3. Crear Action Bar
        GameObject actionObj = new GameObject("ActionBar");
        actionObj.transform.SetParent(canvasVal.transform, false);
        RectTransform actionRect = actionObj.AddComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0.5f, 0);
        actionRect.anchorMax = new Vector2(0.5f, 0);
        actionRect.pivot = new Vector2(0.5f, 0);
        actionRect.anchoredPosition = new Vector2(0, 20);

        ActionBarUI actionScript = actionObj.AddComponent<ActionBarUI>();

        // Slot 1
        GameObject slot1 = CreateSlot("Slot1", "1", actionObj.transform, new Vector2(-60, 0));
        actionScript.GetType().GetField("iconSlot1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(actionScript, slot1.transform.Find("Icon").GetComponent<Image>());
        actionScript.GetType().GetField("textSlot1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(actionScript, slot1.transform.Find("Hotkey").GetComponent<TextMeshProUGUI>());

        // Slot 2
        GameObject slot2 = CreateSlot("Slot2", "2", actionObj.transform, new Vector2(60, 0));
        actionScript.GetType().GetField("iconSlot2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(actionScript, slot2.transform.Find("Icon").GetComponent<Image>());
        actionScript.GetType().GetField("textSlot2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(actionScript, slot2.transform.Find("Hotkey").GetComponent<TextMeshProUGUI>());

        // 4. Crear Class Selection Panel
        GameObject classPanel = new GameObject("ClassSelectionPanel");
        classPanel.transform.SetParent(canvasVal.transform, false);
        RectTransform classPanelRect = classPanel.AddComponent<RectTransform>();
        classPanelRect.anchorMin = Vector2.zero;
        classPanelRect.anchorMax = Vector2.one;
        classPanelRect.sizeDelta = Vector2.zero;
        
        Image classPanelBg = classPanel.AddComponent<Image>();
        classPanelBg.color = new Color(0, 0, 0, 0.8f);

        ClassSelectionUI classScript = classPanel.AddComponent<ClassSelectionUI>();
        classScript.GetType().GetField("panel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(classScript, classPanel);

        // Título
        GameObject title = CreateText("Title", "Selecciona tu Clase", classPanel.transform);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(400, 60);
        title.GetComponent<TextMeshProUGUI>().fontSize = 32;

        // Botones
        GameObject btnMago = CreateButton("BtnMago", "Mago", classPanel.transform, new Vector2(-150, 50));
        GameObject btnPaladin = CreateButton("BtnPaladin", "Paladin", classPanel.transform, new Vector2(150, 50));
        GameObject btnClerigo = CreateButton("BtnClerigo", "Clerigo", classPanel.transform, new Vector2(-150, -50));
        GameObject btnCazador = CreateButton("BtnCazador", "Cazador", classPanel.transform, new Vector2(150, -50));

        classScript.GetType().GetField("btnMago", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(classScript, btnMago.GetComponent<Button>());
        classScript.GetType().GetField("btnPaladin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(classScript, btnPaladin.GetComponent<Button>());
        classScript.GetType().GetField("btnClerigo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(classScript, btnClerigo.GetComponent<Button>());
        classScript.GetType().GetField("btnCazador", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(classScript, btnCazador.GetComponent<Button>());

        // 5. Crear EventSystem si no existe (necesario para que funcionen los botones)
        if (GameObject.Find("EventSystem") == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ EventSystem creado.");
        }

        Debug.Log("✅ Combat UI Created Successfully!");
    }

    static GameObject CreateSlider(string name, Color color, Transform parent, Vector2 pos)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 20);
        rect.anchoredPosition = pos;

        Slider slider = sliderObj.AddComponent<Slider>();
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, 0); // Padding

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = color;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;

        slider.targetGraphic = bgImg;
        slider.fillRect = fillRect;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;

        return sliderObj;
    }

    static GameObject CreateText(string name, string content, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 14;
        tmp.color = Color.white;
        
        return textObj;
    }

    static GameObject CreateSlot(string name, string hotkey, Transform parent, Vector2 pos)
    {
        GameObject slotOrbi = new GameObject(name);
        slotOrbi.transform.SetParent(parent, false);
        RectTransform rect = slotOrbi.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50, 50);
        rect.anchoredPosition = pos;

        Image bg = slotOrbi.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f);

        // Icon
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(slotOrbi.transform, false);
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-5, -5);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = Color.white;
        iconImg.enabled = false; // Hidden until ability is set

        // Hotkey
        GameObject key = CreateText("Hotkey", hotkey, slotOrbi.transform);
        RectTransform keyRect = key.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0, 0);
        keyRect.anchorMax = new Vector2(1, 0.4f);
        
        return slotOrbi;
    }

    static GameObject CreateButton(string name, string text, Transform parent, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 60);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject txtObj = CreateText("Text", text, btnObj.transform);
        txtObj.GetComponent<TextMeshProUGUI>().fontSize = 20;
        txtObj.GetComponent<TextMeshProUGUI>().color = Color.white;

        return btnObj;
    }
}
