using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Static class containing functions to create GUI.
/// </summary>
public static class GUIManager
{
    /// +++ FUNCTIONS +++ ///

    /// <summary>
    /// Function called when button is pressed.
    /// Update this functions when creating a new button.
    /// </summary>
    /// <param name="name">Name of button.</param>
    public static void OnButtonClick(string name)
    {
        switch(name)
        {
            
        }
    }

    /// <summary>
    /// Create canvas.
    /// </summary>
    /// <param name="scene">Scene in which game object will be contained.</param>
    /// <param name="name">Name of game object.</param>
    /// <returns>Returns new game object.</returns>
    public static GameObject CreateCanvas(string name)
    {
        GameObject gameObject = new GameObject(name);

        gameObject.transform.position = new Vector3(0, 0, 0);

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        gameObject.AddComponent<CanvasScaler>();

        gameObject.AddComponent<GraphicRaycaster>();

        return gameObject;
    }

    /// <summary>
    /// Create button.
    /// </summary>
    /// <param name="parentGO">Parent game object</param>
    /// <param name="x">Position X from bottom left corner (range [0,1]).</param>
    /// <param name="y">Position Y from bottom left corner (range [0,1]).</param>
    /// <param name="width">Width (range [0,1]).</param>
    /// <param name="height">Height (range [0,1]).</param>
    /// <param name="texture">Texture of game object, transparent if null.</param>
    /// <param name="name">Name of game object.</param>
    /// <returns>Returns new game object.</returns>
    public static GameObject CreateButton(GameObject parentGO, float x, float y, float width, float height, Texture2D texture, string name, UnityEngine.Events.UnityAction<PointerEventData> action = null, EventTriggerType type = EventTriggerType.PointerClick)
    {

		// We want the aspect ratio of the button to always be that of the texture, so we adjust the width.
        width = AdjustWidth(width, texture);

        // Check dependencies.
        Debug.Assert(parentGO.GetComponent<Canvas>());

        // Create new game object.
        GameObject gameObject = new GameObject(name);

        gameObject.AddComponent<CanvasRenderer>();

		//We set the texture of the button, if we haven't provided one we use a defualt.
        Image image = gameObject.AddComponent<Image>();
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.0F, 0.0F));
            image.sprite = sprite;
        }
        else
        {
            image.color = new Color(0, 0, 0, 0);
        }

        Button button = gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        
		//We position the button correctly.
        RectTransform buttonRect = gameObject.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(x * Screen.width, y * Screen.height);
        buttonRect.sizeDelta = new Vector2(width * Screen.width, height * Screen.height);


        if (action == null)
			//If we didn't provide and action, we use the default one.
            button.onClick.AddListener(() => OnButtonClick(name));
        else
        {
            AddEvent(gameObject, action, type);
        }

        gameObject.transform.SetParent(parentGO.transform);

        return gameObject;

    }

    /// <summary>
    /// Create Input textfield.
    /// </summary>
    /// <param name="parentGO">Parent game object</param>
    /// <param name="x">Position X from bottom left corner (range [0,1]).</param>
    /// <param name="y">Position Y from bottom left corner (range [0,1]).</param>
    /// <param name="width">Width (range [0,1]).</param>
    /// <param name="height">Height (range [0,1]).</param>
    /// <param name="name">Name of game object.</param>
    /// <returns>Returns new game object.</returns>
    public static GameObject CreateInputTextfield(GameObject parentGO, float x, float y, float width, float height, string name, string defaultText = "write here...", string fontName = "DefaultFont", Texture2D texture = null)
    {
        // Check dependencies.
        Debug.Assert(parentGO.GetComponent<Canvas>());

        // Check name.
        Debug.Assert(!GameObject.Find(name));

        // Create new game object.
        GameObject gameObject = new GameObject(name);
        GameObject backgroundObject = new GameObject(name + "BACKGROUND");

        gameObject.AddComponent<CanvasRenderer>();
        backgroundObject.AddComponent<CanvasRenderer>();

		//We set the texture of the input field, if we haven't provided one we use a defualt.
        Image image = backgroundObject.AddComponent<Image>();
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.0F, 0.0F));
            image.sprite = sprite;
        }
        else
        {
            texture = Resources.Load<Texture2D>("Textures/InputFieldDefault");
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.0F, 0.0F));
            image.sprite = sprite;
        }

		//We create the black outline of the inputfield.
        UnityEngine.UI.Outline outline = backgroundObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

		//We set the position of the inputfield.
        RectTransform imageRect = backgroundObject.GetComponent<RectTransform>();
        imageRect.anchoredPosition = new Vector2(x * Screen.width, y * Screen.height);
        imageRect.sizeDelta = new Vector2(width * Screen.width, height * Screen.height);

		//We set the position of the input text.
        Text textLabel = gameObject.AddComponent<Text>();
        RectTransform textRect = gameObject.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(x * Screen.width, y * Screen.height);
        textRect.sizeDelta = new Vector2(width * Screen.width, height * Screen.height);

		//We link the input field to the label and background.
        InputField inputField = gameObject.AddComponent<InputField>();
        inputField.textComponent = textLabel;
        inputField.text = defaultText;
        inputField.targetGraphic = image;

		//We set the font, we also make sure it scales with the height of the inputfield.
        float fontScale = (height * Screen.height) / (621 * 0.2f) * 80;
        textLabel.fontSize = Mathf.FloorToInt(fontScale);
        textLabel.font = Resources.Load<Font>(fontName);
        textLabel.color = Color.black;
        textLabel.alignment = TextAnchor.MiddleLeft;

        gameObject.transform.SetParent(backgroundObject.transform);
        backgroundObject.transform.SetParent(parentGO.transform);

        return gameObject;

    }

	//Sets a function callback when you press a button.
    public static void AddEvent(GameObject target, UnityEngine.Events.UnityAction<PointerEventData> action = null, EventTriggerType type = EventTriggerType.PointerClick)
    {

        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => { action((PointerEventData)data); });
        trigger.triggers.Add(entry);

    }

    /// <summary>
    /// Create text.
    /// </summary>
    /// <param name="parentGO">Parent game object</param>
    /// <param name="x">Position X from bottom left corner (range [0,1]).</param>
    /// <param name="y">Position Y from bottom left corner (range [0,1]).</param>
    /// <param name="width">Width (range [0,1]).</param>
    /// <param name="height">Height (range [0,1]).</param>
    /// <param name="text">Text to write.</param>
    /// <param name="name">Name of game object.</param>
    /// <returns>Returns new game object.</returns>
    public static GameObject CreateText(GameObject parentGO, float x, float y, float width, float height, string text, string name, string fontName = "LemonMilkbold", float sizeOffset = 0, TextAnchor align = TextAnchor.MiddleCenter)
    {
        // Check dependencies.
        Debug.Assert(parentGO.GetComponent<Canvas>());

        // Check name.
        Debug.Assert(!GameObject.Find(name));

        // Create new game object.
        GameObject gameObject = new GameObject(name);

        gameObject.AddComponent<CanvasRenderer>();

        Text textComp = gameObject.AddComponent<Text>();
        textComp.text = text;

		//We set the font, we also make sure it scales with the height of the inputfield.
        float fontScale = (height * Screen.height) / (621 * 0.2f) * 90 - sizeOffset;
        textComp.fontSize = Mathf.FloorToInt(fontScale);
        textComp.font = Resources.Load<Font>(fontName);
        textComp.color = Color.black;
        textComp.alignment = align;

		//We set the position of the text.
        RectTransform buttonRect = gameObject.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(x * Screen.width, y * Screen.height);
        buttonRect.sizeDelta = new Vector2(Screen.width, height * Screen.height);

        gameObject.transform.SetParent(parentGO.transform);

        return gameObject;

    }

	public static void CreateImage(GameObject parentGO, float x, float y, float width, float height, string name, Texture2D texture, bool forceAspectRatio = true)
    {

		if(forceAspectRatio)
	        width = AdjustWidth(width, texture);

        // Check dependencies.
        Debug.Assert(parentGO.GetComponent<Canvas>());

        // Check name.
        Debug.Assert(!GameObject.Find(name));

        // Create new game object.
        GameObject gameObject = new GameObject(name);

        gameObject.AddComponent<CanvasRenderer>();

        Image image = gameObject.AddComponent<Image>();
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.0F, 0.0F));
            image.sprite = sprite;
        }
        else
        {
            texture = Resources.Load<Texture2D>("Textures/DefaultTexture");
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.0F, 0.0F));
            image.sprite = sprite;
        }

        RectTransform imageRect = gameObject.GetComponent<RectTransform>();
        imageRect.anchoredPosition = new Vector2(x * Screen.width, y * Screen.height);
        imageRect.sizeDelta = new Vector2(width * Screen.width, height * Screen.height);

        gameObject.transform.SetParent(parentGO.transform);

    }

	//Adjusts the width to that the aspect ratio is kept.
    private static float AdjustWidth(float width, Texture2D texture)
    {

        float aspectRatio = (float)texture.width / texture.height;
        aspectRatio /= (float)Screen.width / Screen.height;

        return width * aspectRatio;

    }

    /// --- FUNCTIONS --- ///
}
