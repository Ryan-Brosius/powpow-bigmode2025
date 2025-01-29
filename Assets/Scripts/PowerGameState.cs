using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

public class PowerGameState : MonoBehaviour
{
    [Tooltip("Power String")]
    [SerializeField] private PowRegexWord POWObject;

    public List<PowData> PowData { get; private set; }
    public static PowerGameState Instance { get; private set; }

    public string PowString
    {
        get { return POWObject.CurrentPowString; }
        set { POWObject.CurrentPowString = value; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        PowData = PowRegexWord.ExtractPowData(PowString);
    }

    private void Start()
    {
        /*Debug.Log($"Power String: {PowString}");
        foreach (var bullet in PowData)
        {
            Debug.Log(bullet);
        }*/
    }

    private void Update()
    {
        PowData = PowRegexWord.ExtractPowData(PowString);
    }
}
