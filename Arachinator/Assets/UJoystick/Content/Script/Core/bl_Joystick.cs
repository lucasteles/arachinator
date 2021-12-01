﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class bl_Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{

    [Header("Settings")]
    [SerializeField, Range(1, 15)]private float Radio = 5;//the ratio of the circumference of the joystick
    [SerializeField, Range(0.01f, 1)]private float SmoothTime = 0.5f;//return to default position speed
    [SerializeField, Range(0.5f, 4)] private float OnPressScale = 1.5f;//return to default position speed
    public Color NormalColor = new Color(1, 1, 1, 1);
    public Color PressColor = new Color(1, 1, 1, 1);
    [SerializeField, Range(0.1f, 5)]private float Duration = 1;

    [Header("Reference")]
    [SerializeField]private RectTransform StickRect;//The middle joystick UI
    [SerializeField] private RectTransform CenterReference;

    //Privates
    private Vector3 DeathArea;
    private Vector3 currentVelocity;
    private bool isFree = false;
    private int lastId = -2;
    private Image stickImage;
    private Image backImage;
    private Canvas m_Canvas;
    private float diff;
    public Vector3 PressScaleVector;

    Life playerLife;
    void Awake()
    {
        if (FindObjectOfType<Player>() is { } player)
        {
            playerLife = player.GetComponent<Life>();
            playerLife.onDeath += PlayerLifeOnonDeath;
        }
    }
    void OnDestroy() => playerLife.onDeath -= PlayerLifeOnonDeath;

    void PlayerLifeOnonDeath(Life obj) => Reset();

    void Reset()
    {
        StopAllCoroutines();
        isFree = false;
        lastId = -2;
        StickRect.localScale = Vector3.one;
        StickRect.position = DeathArea;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (StickRect == null)
        {
            Debug.LogError("Please add the stick for joystick work!.");
            this.enabled = false;
            return;
        }

        if (transform.root.GetComponent<Canvas>() != null)
        {
            m_Canvas = transform.root.GetComponent<Canvas>();
        }
        else if (transform.root.GetComponentInChildren<Canvas>() != null)
        {
            m_Canvas = transform.root.GetComponentInChildren<Canvas>();
        }
        else
        {
            Debug.LogError("Required at lest one canvas for joystick work.!");
            this.enabled = false;
            return;
        }
       
        //Get the default area of joystick
        DeathArea = CenterReference.position;
        diff = CenterReference.position.magnitude;
        PressScaleVector = new Vector3(OnPressScale, OnPressScale, OnPressScale);
        if (GetComponent<Image>() != null)
        {
            backImage = GetComponent<Image>();
            stickImage = StickRect.GetComponent<Image>();
            backImage.CrossFadeColor(NormalColor, 0.1f, true, true);
            stickImage.CrossFadeColor(NormalColor, 0.1f, true, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        DeathArea = CenterReference.position;
        //If this not free (not touched) then not need continue
        if (!isFree)
            return;

        if (Mathf.Abs(lastId) > 100f) Reset();
        //Return to default position with a smooth movement
        StickRect.position = Vector3.SmoothDamp(StickRect.position, DeathArea, ref currentVelocity, smoothTime);
        //When is in default position, we not need continue update this
        if (Vector3.Distance(StickRect.position, DeathArea) < .1f)
        {
            isFree = false;
            StickRect.position = DeathArea;
        }
    }

    /// <summary>
    /// When click here event
    /// </summary>
    /// <param name="data"></param>
    public void OnPointerDown(PointerEventData data)
    {
        //strange bug with huge lastIds
        if (Mathf.Abs(lastId) > 100f) Reset();
        //Detect if is the default touchID
        if (lastId != -2) return;

        //then get the current id of the current touch.
        //this for avoid that other touch can take effect in the drag position event.
        //we only need get the position of this touch
        lastId = data.pointerId;
        StopAllCoroutines();
        StartCoroutine(ScaleJoysctick(true));
        OnDrag(data);
        if (backImage == null) return;
        backImage.CrossFadeColor(PressColor, Duration, true, true);
        stickImage.CrossFadeColor(PressColor, Duration, true, true);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void OnDrag(PointerEventData data)
    {
        //If this touch id is the first touch in the event
        if (data.pointerId != lastId) return;

        isFree = false;
        //Get Position of current touch
        var position = bl_JoystickUtils.TouchPosition(m_Canvas, data.position);

        //Rotate into the area circumferential of joystick
        if (Vector2.Distance(DeathArea, position) < radio)
        {
            StickRect.position = position;
        }
        else
        {
            StickRect.position = DeathArea + (position - DeathArea).normalized * radio;
        }
    }

    /// <summary>
    /// When touch is Up
    /// </summary>
    /// <param name="data"></param>
    public void OnPointerUp(PointerEventData data)
    {
        isFree = true;
        currentVelocity = Vector3.zero;
        //leave the default id again
        if (data.pointerId != lastId) return;

        //-2 due -1 is the first touch id
        lastId = -2;
        StopAllCoroutines();
        StartCoroutine(ScaleJoysctick(false));
        if (backImage == null) return;
        backImage.CrossFadeColor(NormalColor, Duration, true, true);
        stickImage.CrossFadeColor(NormalColor, Duration, true, true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ScaleJoysctick(bool increase)
    {
        float time = 0;
        while (time < Duration)
        {
            StickRect.localScale = Vector3.Lerp(
                StickRect.localScale,
                increase
                    ? PressScaleVector
                    : Vector3.one, time / Duration);

            time += Time.deltaTime;
            yield return null;
        }
    }
    
    float radio => Radio * 5 + Mathf.Abs(diff - CenterReference.position.magnitude);
    float smoothTime => 1 - SmoothTime;

    /// <summary>
    /// Value Horizontal of the Joystick
    /// Get this for get the horizontal value of joystick
    /// </summary>
    public float Horizontal => (StickRect.position.x - DeathArea.x) / Radio;

    /// <summary>
    /// Value Vertical of the Joystick
    /// Get this for get the vertical value of joystick
    /// </summary>
    public float Vertical => (StickRect.position.y - DeathArea.y) / Radio;
}