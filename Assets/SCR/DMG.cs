using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DMG : MonoBehaviour
{
    public TextMeshPro texto;
    private float Duration = 1f;
    private float Fade = 1f;
    private Vector3 MovementDir = new Vector3(0,1);
    private float MovementSpeed = 3f;
    public void InitDamage(float dmg, float scale)
    {
        texto.text = dmg.ToString("0");
        texto.color = Color.red;
        transform.localScale = new Vector3(scale, scale, 1);
        MovementDir = new Vector3(Random.Range(-0.3f,0.3f), Random.Range(0.2f, 0.3f));
    }
    public void InitWords(string str, float dur, Color col)
    {
        texto.text = str;
        texto.color = col;
        Duration = dur;
        transform.localScale = new Vector3(0.5f, 0.5f, 1);
        MovementDir = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.2f, 0.3f));
    }
    private void Update()
    {
        Duration -= CO.co.GetWorldSpeedDelta();
        transform.position += MovementSpeed * MovementDir * CO.co.GetWorldSpeedDelta();
        MovementSpeed -= 0.5f * CO.co.GetWorldSpeedDelta();
        if (Duration < 0f)
        {
            Fade -= CO.co.GetWorldSpeedDelta();
            texto.color = new Color(texto.color.r, texto.color.g, texto.color.b, Fade);
            if (Fade < 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
