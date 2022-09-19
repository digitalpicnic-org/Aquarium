using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoralShader : MonoBehaviour
{
    [SerializeField]private MeshRenderer _renderer;
    [SerializeField]private Texture sprite;
    public float waveSpeed;
    public float waveFreq;
    public float waveDist;
    public bool isFlip;
    void Start()
    {
        _renderer.material.SetFloat("_Random", UnityEngine.Random.Range(-3.14f, 3.14f));
        UpdateShader();
    }

    private void Update() {
        UpdateShader();
    }

    private void UpdateShader(){
        _renderer.material.SetTexture("_BaseMap", sprite);
        _renderer.material.SetFloat("_WaveSpeed", waveSpeed);
        _renderer.material.SetFloat("_WaveFreq", waveFreq);
        _renderer.material.SetFloat("_WaveDist", waveDist);
        _renderer.material.SetInt("_Flip", isFlip ? 1 : 0);
    }
}
