using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{

    private Vector2 fixedPosition = Vector2.zero;
    public int pointerId;
    bool isTouched;
    bool isFirstMultiTouch = true;

    protected override void Start()
    {
        fixedPosition = background.anchoredPosition;
        base.Start();
        background.gameObject.SetActive(true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);

        GameManager.Instance.GetCurrentPlayer.GetComponent<PlayerCombatManager>().canShoot = true;

        base.OnPointerDown(eventData);
            

    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        background.anchoredPosition = fixedPosition;

        //background.gameObject.SetActive(false);
        base.OnPointerUp(eventData);

    }
}