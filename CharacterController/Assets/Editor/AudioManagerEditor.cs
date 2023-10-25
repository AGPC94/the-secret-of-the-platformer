using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("CreateSoundsAssets"))
        {
            CreateSoundsAssets();
        }
    }

    void CreateSoundsAssets()
    {
        AudioClip[] effects = Resources.LoadAll("AudioManager/Audio/Effects", typeof(AudioClip)).Cast<AudioClip>().ToArray();
        AudioClip[] music = Resources.LoadAll("AudioManager/Audio/Music", typeof(AudioClip)).Cast<AudioClip>().ToArray();

        CreateSounds(effects);
        CreateSounds(music, true);
    }

    void CreateSounds(AudioClip[] audios, bool isMusic = false)
    {
        foreach (AudioClip audio in audios)
        {
            Sound asset = CreateInstance<Sound>();

            asset.clip = audio;

            if (isMusic)
            {
                asset.loop = true;
                AssetDatabase.CreateAsset(asset, "Assets/Resources/AudioManager/Sounds/Music/" + audio.name + ".asset");
            }
            else
            {
                asset.loop = false;
                AssetDatabase.CreateAsset(asset, "Assets/Resources/AudioManager/Sounds/Effects/" + audio.name + ".asset");
            }

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
