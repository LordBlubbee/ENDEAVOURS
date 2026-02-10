using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class VCX : MonoBehaviour
{
    public TextMeshPro texto;
    private float Duration = 0.8f;
    private float Fade = 1f;
    private Vector3 MovementDir = new Vector3(0,1);
    private float MovementSpeed = 3f;
    float Scale = 1f;
    int Writeout = 0;

    AudioClip Voice;
    Transform Follow = null;
    Vector3 Offset = Vector3.zero;
    Vector3 Shake = Vector3.zero;
    VoiceStyles Style = VoiceStyles.NONE;
    public enum VoiceStyles
    {
        NONE,
        SCARED_SHAKE,
        SHOUT_SHAKE,
        SHAKE,
        ROBOTIC
    }
    public void InitVoice(string str, Transform tr, VoiceStyles style, AudioClip voice, Color col)
    {
        texto.text = str;
        texto.color = col;
        Duration = 2.5f + str.Length*0.02f;
        Follow = tr;
        Voice = voice;

        Scale = 0.5f;
        float scale = Scale * 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale * 0.6f, scale * 0.6f, 1);
        MovementDir = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.2f, 0.3f));

        Offset = new Vector3(0, 3.5f);
        Style = style;
    }

    float WriteoutTimer = 0f;
    int speakLetter = 0;

    int ShakePhase = 1;
    float CurrentShakeAmount = 0f;
    private void ShakeFrame(float maxShake, float shakeSpeed)
    {
        if (maxShake <= 0f) return;
        CurrentShakeAmount += shakeSpeed * ShakePhase * Time.deltaTime;
        if (CurrentShakeAmount > maxShake && ShakePhase > 0) ShakePhase *= -1;
        if (CurrentShakeAmount < -maxShake && ShakePhase < 0) ShakePhase *= -1;
        Shake = new Vector3(0, CurrentShakeAmount);
    }

    int RotationShakePhase = 1;
    float CurrentRotationShakeAmount = 0f;
    private void ShakeRotFrame(float maxShake, float shakeSpeed)
    {
        if (maxShake <= 0f) return;
        CurrentRotationShakeAmount += shakeSpeed * RotationShakePhase * Time.deltaTime;
        if (CurrentRotationShakeAmount > maxShake && RotationShakePhase > 0) RotationShakePhase *= -1;
        if (CurrentRotationShakeAmount < -maxShake && RotationShakePhase < 0) RotationShakePhase *= -1;
        transform.rotation = Quaternion.Euler(0, 0, CurrentRotationShakeAmount);
    }

    float DurationFactor()
    {
        return Duration * 0.5f;
    }
    private void Update()
    {
        float WriteoutSpeed = 0.025f;
        switch (Style)
        {
            case VoiceStyles.SCARED_SHAKE:
                WriteoutSpeed = 0.035f;
                ShakeFrame(0.2f * DurationFactor(), 2.5f * DurationFactor());
                ShakeRotFrame(2f * DurationFactor(), 6f * DurationFactor());
                break;
            case VoiceStyles.SHOUT_SHAKE:
                WriteoutSpeed = 0.017f;
                ShakeFrame(0.3f * DurationFactor(), 24f * DurationFactor());
                ShakeRotFrame(3f * DurationFactor(), 16f * DurationFactor());
                break;
            case VoiceStyles.SHAKE:
                WriteoutSpeed = 0.02f;
                ShakeFrame(0.15f * DurationFactor(), 12f * DurationFactor());
                ShakeRotFrame(1.5f * DurationFactor(), 8f * DurationFactor());
                break;
        }
        Duration -= CO.co.GetWorldSpeedDelta() * 0.9f;
        WriteoutTimer += CO.co.GetWorldSpeedDelta();
        int totalChars = texto.text.Length;
        if (WriteoutTimer > WriteoutSpeed)
        {
            WriteoutTimer -= WriteoutSpeed;
            Writeout++;
            char c = texto.text[Mathf.Clamp(Writeout - 1, 0, totalChars - 1)];
            if (Voice && char.IsLetterOrDigit(c))
            {
                speakLetter--;
                if (speakLetter < 0)
                {
                    speakLetter = Random.Range(2, 4);
                    AudioClip clip = Voice;
                    AUDCO.aud.PlayVCX(clip, transform.position);
                }
            }
        }
        texto.maxVisibleCharacters = Writeout;

        float scale = Scale * 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale, scale, 1);
        Offset += MovementSpeed * MovementDir * scale * CO.co.GetWorldSpeedDelta();
        transform.position = Follow.position + Offset + Shake;

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
