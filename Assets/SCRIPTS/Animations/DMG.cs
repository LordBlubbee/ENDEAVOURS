using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DMG : MonoBehaviour
{
    public TextMeshPro texto;
    private float Duration = 0.8f;
    private float Fade = 1f;
    private Vector3 MovementDir = new Vector3(0,1);
    private float MovementSpeed = 3f;
    float Scale = 1f;
    public void InitNumber(float dmg, float scale, Color col)
    {
        texto.text = dmg.ToString("0");
        texto.color = col;

        Scale = scale;
        scale *= 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale, scale, 1);

        MovementDir = new Vector3(Random.Range(-0.6f,0.6f), Random.Range(0.4f, 0.5f));
    }
    public void InitWords(string str, float dur, Color col)
    {
        texto.text = str;
        texto.color = col;
        Duration = dur;

        Scale = 0.9f;
        float scale = Scale * 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale * 0.6f, scale * 0.6f, 1);
        MovementDir = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.2f, 0.3f));
    }
    public void InitVoice(string str, float dur, Color col)
    {
        texto.text = str;
        texto.color = col;
        Duration = dur;

        Scale = 0.6f;
        float scale = Scale * 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale * 0.6f, scale * 0.6f, 1);
        MovementDir = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.2f, 0.3f));
    }

    private void Update()
    {
        Duration -= CO.co.GetWorldSpeedDelta();

        float scale = Scale * 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale, scale, 1);
        transform.position += MovementSpeed * MovementDir * scale * CO.co.GetWorldSpeedDelta();

        MovementSpeed = Mathf.Max(0,MovementSpeed- 1f * CO.co.GetWorldSpeedDelta());
        if (Duration < 0f)
        {
            Fade -= CO.co.GetWorldSpeedDelta() * 2f;
            texto.color = new Color(texto.color.r, texto.color.g, texto.color.b, Fade);
            if (Fade < 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
