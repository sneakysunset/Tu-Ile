using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RessourcesManager : MonoBehaviour
{
    public int wood, rock;
    public int tileCost;
    public TextMeshProUGUI woodText, rockText;
    private void Update()
    {
        woodText.text = wood.ToString();
        rockText.text = rock.ToString();
    }
}
