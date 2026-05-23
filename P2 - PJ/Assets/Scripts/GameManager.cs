using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager gameM;
    public bool isGameOver = false;

    public List<AudioClip> soundtrack;

    public AudioSource music;
    public AudioSource SFX;

    public AudioClip botonPresionado;
    public FadeInOut fade;

    void Start()
    {
        // Asegurarse de que el GameManager aplique el volumen guardado nada más abrir el juego
        if (music != null)
            music.volume = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);

        if (SFX != null)
            SFX.volume = PlayerPrefs.GetFloat("VolumenSFX", 0.5f);
        
        fade = GetComponentInChildren<FadeInOut>();
        fade.FadeIn(1f);
        fade.FadeOut(1f);
    }

    void Update()
    {  
        // Resetear datos
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.B))
        {
            Debug.Log("Reset datos");
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        if (gameM == null)
        {
            gameM = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (gameM != this)
        {
            Destroy(gameObject);
        }
    }

    public void TogglePause()
    {
        if (music.isPlaying)
        {
            music.Pause();
        }
        else
        {
            music.UnPause();
        }
    }

    public void CambiarCancion(int index)
    {
        music.clip = soundtrack[index];
        music.loop = true;        
        music.Play();
        StartCoroutine(FadeInMusic());
    }

    public void ReiniciarCancion()
    {
        music.Stop();
        music.Play();
    }

    private IEnumerator FadeInMusic()
    {
        float volumenOriginal = music.volume;
        music.volume = 0;
        while (music.volume < volumenOriginal)
        {
            music.volume += 0.1f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void BotonPresionadoSFX()
    {
        SFX.pitch = 1;
        SFX.PlayOneShot(botonPresionado);
    }

    public void ReproducirSonido(AudioClip sonido, float pitchMin)
    {
        System.Random r = new System.Random();
        SFX.pitch = 0.8f + pitchMin + (float)r.NextDouble();
        SFX.PlayOneShot(sonido);
    }

    public IEnumerator CambiarEscena(int index, float duracionFade)
    {
        Debug.Log("Cambio escena");
        yield return fade.FadeIn(duracionFade);
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(index);        
        yield return new WaitForSeconds(0.1f);
        Debug.Log("FADE OUT");
        yield return fade.FadeOut(duracionFade);
    }    
}