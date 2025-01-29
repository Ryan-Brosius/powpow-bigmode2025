using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

[CreateAssetMenu(menuName = "POWER String")]
public class PowRegexWord : ScriptableObject
{
    static string validPattern = @"(P+O+W+)+";
    static string matchesPattern = @"(P+O+W+)";
    static Regex validRegex = new Regex(validPattern, RegexOptions.IgnoreCase);
    static Regex matchesRegex = new Regex(matchesPattern, RegexOptions.IgnoreCase);

    public List<PowData> PowDataList { get; private set; }
    [SerializeField] private string defaultPowString = "POW";
    [SerializeField] private string currentPowString;

    public string CurrentPowString
    {
        get { return currentPowString; }
        set
        {
            currentPowString = value;
            PowDataList = ExtractPowData(value);
        }
    }

    private void OnEnable()
    {
        CurrentPowString = defaultPowString;
        PowDataList = ExtractPowData(CurrentPowString);
    }

    static bool ValidPow(string s)
    {
        return validRegex.IsMatch(s);
    }

    public static List<PowData> ExtractPowData(string s)
    {
        List<PowData> powDataList = new List<PowData>();

        if (!ValidPow(s))
        {
            return powDataList;
        }

        MatchCollection matches = matchesRegex.Matches(s);

        foreach (Match match in matches)
        {
            string pow = match.Value;

            int pCount = pow.Count(c => char.ToUpper(c) == 'P');
            int oCount = pow.Count(c => char.ToUpper(c) == 'O');
            int wCount = pow.Count(c => char.ToUpper(c) == 'W');

            powDataList.Add(new PowData
            {
                BulletSize = pCount,
                BulletDamage = oCount,
                BulletsPerShot = wCount
            });
        }

        return powDataList;
    }

    private void OnValidate()
    {
        PowDataList = ExtractPowData(currentPowString);
    }
}
