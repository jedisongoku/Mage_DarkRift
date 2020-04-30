using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyAnimation : MonoBehaviour
{
    [SerializeField] private GameObject[] Icons;
    [SerializeField] private Transform[] expandLocations;
    [SerializeField] private GameObject targetLocation;

    [SerializeField] private float lerpSpeed = 16;

    private int currentAmount;
    private int amountAdded;

    private float timer;
    // Start is called before the first frame update
    void OnEnable()
    {
        foreach(var item in Icons)
        {
            item.transform.position = transform.position;
            item.SetActive(true);
        }
        timer = 0;
        StartCoroutine(Expand());
        Handheld.Vibrate();
    }

    public void SetAmount(int current, int added)
    {
        currentAmount = current;
        amountAdded = added;
    }


    IEnumerator Expand()
    {
        for(int i = 0; i < Icons.Length; i++)
        {
            Icons[i].transform.position = Vector2.Lerp(Icons[i].transform.position, expandLocations[i].transform.position, Time.deltaTime * lerpSpeed / 2);
        }
        timer += Time.deltaTime;
        yield return new WaitForSeconds(0);

        if(timer < 0.5f)
        {
            StartCoroutine(Expand());
        }
        else
        {
            timer = 0;
            StartCoroutine(StartMovingToTarget(0));
            Invoke("AutoDisable", 1.5f);
        }
    }

    IEnumerator StartMovingToTarget(int i)
    {
        StartCoroutine(MoveToTarget(i));

        yield return new WaitForSeconds(0.1f);

        if(i < Icons.Length - 1)
        {  
            i++;
            StartCoroutine(StartMovingToTarget(i));
        }
        
    }

    IEnumerator MoveToTarget(int i)
    {
        Icons[i].transform.position = Vector2.Lerp(Icons[i].transform.position, targetLocation.transform.position, Time.deltaTime * lerpSpeed);
        Icons[i].transform.rotation = Quaternion.Lerp(Icons[i].transform.rotation, targetLocation.transform.rotation, Time.deltaTime * lerpSpeed);
        yield return new WaitForSeconds(0);

        if (Vector2.Distance(Icons[i].transform.position, targetLocation.transform.position) > 0.05)
        {
            StartCoroutine(MoveToTarget(i));
        }
        else
        {
            currentAmount += amountAdded / 5;
            HUDManager.Instance.GemCurrencyTextUpdate(currentAmount);
            Handheld.Vibrate();
            Icons[i].SetActive(false);
        }

    }

    void AutoDisable()
    {
        gameObject.SetActive(false);
    }
}
