using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource _musicSource;
    public AudioSource villagerSource;
    public AudioSource tileSource;

    private List<AudioClip> villagerSoundsList = new List<AudioClip>();
    private List<AudioClip> tillList = new List<AudioClip>();
    private List<AudioClip> untillList = new List<AudioClip>();
    private List<AudioClip> placeList = new List<AudioClip>();
    private List<AudioClip> breakList = new List<AudioClip>();

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
    }

    private void Start()
    {
        villagerSoundsList = villagerSource.GetComponent<VillagerSounds>().idleSounds;
        tillList = tileSource.GetComponent<TileSounds>().tillTiles;
        untillList = tileSource.GetComponent<TileSounds>().untillTiles;
        placeList = tileSource.GetComponent<TileSounds>().placeBlocks;
        breakList = tileSource.GetComponent<TileSounds>().breakBlocks;
    }

    public void PlaySound(AudioClip clip)
    {
        Debug.Log(clip.ToString());
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public void PlayVillagerDeath()
    {
        villagerSource.clip = villagerSource.GetComponent<VillagerSounds>().death;
        villagerSource.Play();
    }
    public void PlayRandomVillagerIdle()
    {
        int randomIndex = Random.Range(0, villagerSoundsList.Count);
        AudioClip randomClip = villagerSoundsList[randomIndex];
        villagerSource.clip = randomClip;
        villagerSource.Play();
    }

    public void PlayRandomTillTile()
    {
        int randomIndex = Random.Range(0, tillList.Count);
        AudioClip randomClip = tillList[randomIndex];
        tileSource.clip = randomClip;
        tileSource.Play();
    }

    public void PlayRandomUnTillTile()
    {
        int randomIndex = Random.Range(0, untillList.Count);
        AudioClip randomClip = untillList[randomIndex];
        tileSource.clip = randomClip;
        tileSource.Play();
    }
    public void PlayRandomPlaceBlock()
    {
        int randomIndex = Random.Range(0, placeList.Count);
        AudioClip randomClip = placeList[randomIndex];
        tileSource.clip = randomClip;
        tileSource.Play();
    }
    public void PlayRandomBreakBlock()
    {
        int randomIndex = Random.Range(0, breakList.Count);
        AudioClip randomClip = breakList[randomIndex];
        tileSource.clip = randomClip;
        tileSource.Play();
    }
}
