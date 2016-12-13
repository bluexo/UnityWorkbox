using System;
using UnityEngine;
using System.Collections.Generic;

public enum SpriteResourceType { Avatars = 0, Emojis = 1, Flags = 2, Icons = 3 }
public enum PrefabResourceType { Tank = 0, Ammo = 1, Item = 2, UI = 3, Effect = 4, Map = 5 }

public class ResourcesManager : SingletonBehaviour<ResourcesManager>
{
    public static ICollection<Sprite> Avatars
    {
        get { return Instance.spritesResources[SpriteResourceType.Avatars].Values; }
    }
    public static ICollection<Sprite> Emojis
    {
        get { return Instance.spritesResources[SpriteResourceType.Emojis].Values; }
    }
    public static ICollection<Sprite> Flags
    {
        get { return Instance.spritesResources[SpriteResourceType.Flags].Values; }
    }

    private HashSet<GameObject> AllObjects = new HashSet<GameObject>();
    private Dictionary<PrefabResourceType, Dictionary<string, GameObject>> prefabs = new Dictionary<PrefabResourceType, Dictionary<string, GameObject>>();
    private Dictionary<SpriteResourceType, Dictionary<string, Sprite>> spritesResources = new Dictionary<SpriteResourceType, Dictionary<string, Sprite>>();
    public const string defaultCountryName = "United Nations";
    public const string defaultAvatarId = "avatars_default";
    public const string defaultNickName = "YourName";
    public const string defaultMailName = "YourMail";
    public const string defaultEmoji = "cool";

    // Use this for initialization
    private void Awake()
    {
        var prefabTypes = Enum.GetValues(typeof(PrefabResourceType));
        foreach (var val in prefabTypes)
        {
            var type = (PrefabResourceType)val;
            var name = Enum.GetName(typeof(PrefabResourceType), val);
            var res = Resources.LoadAll<GameObject>("Prefabs/" + name);
            if (!prefabs.ContainsKey(type))
            {
                var dict = new Dictionary<string, GameObject>();
                prefabs.Add(type, dict);
            }
            foreach (var go in res)
            {
                prefabs[type].Add(go.name.ToLower(), go);
            }
        }

        var spriteTypes = Enum.GetValues(typeof(SpriteResourceType));
        foreach (var val in spriteTypes)
        {
            var type = (SpriteResourceType)val;
            var name = Enum.GetName(typeof(SpriteResourceType), val);
            var res = Resources.LoadAll<Sprite>("Sprites/" + name);
            if (!spritesResources.ContainsKey(type))
            {
                var dict = new Dictionary<string, Sprite>();
                spritesResources.Add(type, dict);
            }
            foreach (var spr in res)
            {
                spritesResources[type].Add(spr.name.ToLower(), spr);
            }
        }
    }

    public static GameObject LoadPrefab(string name, 
        PrefabResourceType type)
    {
        GameObject obj = null;
        if (!string.IsNullOrEmpty(name))
        {
            name = name.ToLower();
        }
        if (Instance.prefabs.ContainsKey(type)
            && Instance.prefabs[type].ContainsKey(name))
        {
            obj = Instantiate(Instance.prefabs[type][name]);
            Instance.AllObjects.Add(obj);
        }
        if (!obj)
        {
            Debug.LogError("Can not found prefab : " + name);
        }
        return obj;
    }

    public static GameObject[] LoadPrefabs(string name, 
        int count = 1, 
        PrefabResourceType type = PrefabResourceType.Tank)
    {
        GameObject[] objs = new GameObject[count];
        if (!string.IsNullOrEmpty(name))
        {
            name = name.ToLower();
        }
        if (Instance.prefabs.ContainsKey(type)
            && Instance.prefabs[type].ContainsKey(name))
        {
            for (var i = 0; i < count; i++)
            {
                var go = Instantiate(Instance.prefabs[type][name]);
                objs[i] = go;
                Instance.AllObjects.Add(go);
            }
        }
        if (objs.Length < 1)
        {
            Debug.LogError("Can not found Prefabs : " + name);
        }
        return objs;
    }

    public static Sprite LoadSprite(string name, 
        SpriteResourceType type)
    {
        Sprite sprite = null;
        if (name != null)
        {
            name = name.ToLower();
        }
        else
        {
            Debug.LogError("Can not found sprite : " + name);
        }
        if (Instance.spritesResources.ContainsKey(type)
            && Instance.spritesResources[type].ContainsKey(name))
        {
            sprite = Instance.spritesResources[type][name];
        }
        return sprite;
    }

    /// <summary>
    /// 清除生成的所有对象
    /// </summary>
    public static void Clear()
    {
        Resources.UnloadUnusedAssets();
        foreach (var obj in Instance.AllObjects)
        {
            Destroy(obj);
        }
    }
}
