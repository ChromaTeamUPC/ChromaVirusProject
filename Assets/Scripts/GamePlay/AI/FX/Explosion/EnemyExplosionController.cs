using UnityEngine;
using System.Collections;

public class EnemyExplosionController : MonoBehaviour 
{
    public ChromaColor color;
    public GameObject[] colorExplosionPrefabs;

    private AudioSource audioSource;

    private GameObject[] colorExplosion;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        colorExplosion = new GameObject[colorExplosionPrefabs.Length];

        for (int i = 0; i < colorExplosionPrefabs.Length; ++i)
        {
            colorExplosion[i] = GameObject.Instantiate(colorExplosionPrefabs[i], transform.position, colorExplosionPrefabs[i].transform.rotation) as GameObject;
            colorExplosion[i].transform.parent = transform;
            colorExplosion[i].SetActive(false);
        }
    }

    public void Play(ChromaColor color, AudioClip clip)
    {
        this.color = color;
        audioSource.clip = clip;

        Play();
    }

    public void Play()
    {
        colorExplosion[(int)color].SetActive(true);

        if (audioSource.clip != null)
            audioSource.Play();

        StartCoroutine(WaitAndReturnToPool());
    }

    public void PlayAudioOnly(AudioClip clip)
    {
        audioSource.clip = clip;

        if (audioSource.clip != null)
            audioSource.Play();

        StartCoroutine(WaitAndReturnToPool());
    }

    public void PlayAll(AudioClip clip = null)
    {        
        for(int i = 0; i < colorExplosion.Length; ++i)
            colorExplosion[i].SetActive(true);

        audioSource.clip = clip;
        if (audioSource.clip != null)
            audioSource.Play();

        StartCoroutine(WaitAndReturnToPool());
    }

    private IEnumerator WaitAndReturnToPool()
    {
        yield return new WaitForSeconds(3.5f);

        ReturnToPool();
    }

	private void ReturnToPool()
    {
        for (int i = 0; i < colorExplosion.Length; ++i)
            colorExplosion[i].SetActive(false);

        //colorExplosion[(int)color].SetActive(false);

        rsc.poolMng.enemyExplosionPool.AddObject(this);
    }
}
