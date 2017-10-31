
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarChart : MonoBehaviour
{
    public Bar barPrefab;
    List<Bar> bars = new List<Bar>();
    private const int BarMax = 5;

    public void PopulateChart(CarSpecs specs)
    {
        ClearBars();
        AddBar("Top Speed", specs.specTopSpeed);
        AddBar("Acceleration", specs.specAcceleration);
        AddBar("Torgue", specs.specTorgue);
        AddBar("Braking", specs.specBraking);
        AddBar("Handling", specs.specHandling);
        AddBar("Grip", specs.specGrip);
        AddBar("Mass", specs.specMass);
        AddBar("Armor", (float)specs.specArmor);
        AddBar("Weapon Slots", specs.weaponSlots);
    }

    private void AddBar(string specName, float specValue)
    {
        Bar bar = Instantiate(barPrefab, transform) as Bar;
        bar.BarImage.fillAmount = specValue / BarMax;

        bar.Text.SetText(specName);
    }

    /// <summary>
    /// Destroing is ugly but will do for now
    /// </summary>
    private void ClearBars()
    {
        var bars = transform.GetComponentInChildren<Transform>();
        foreach (Transform t in bars)
        {
            Destroy(t.gameObject);
        }
    }
}
  