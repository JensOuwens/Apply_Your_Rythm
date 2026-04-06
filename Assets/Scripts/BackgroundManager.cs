using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField]
    private float playerHitMultiplier; // TODO change this depending on player count, 1 player is 4, 4 players is 1, 3 players is 2
    [SerializeField]
    private float autoIncrement = 0.5f;
    [SerializeField]
    private BackgroundState[] backgrounds;
    [SerializeField]
    private Bucket[] buckets;
    private Composer composer;
    private Metronome metronome;
    private float currentAmount = 0;
    private float state = 1;
    private int lastIndex = -1;

    [Serializable]
    private class BackgroundState
    {
        public float threshold; // percentage between 0 and 1
        [HideInInspector, SerializeField]
        public float nextThreshold;
        public SpriteRenderer background;
        [SerializeField]
        private GameObject parent;
        [SerializeField]
        private ParticleSystem[] particleSystems;
        [SerializeField]
        private LightInstance[] lights;
        private List<LightInstance> currentlyUpdating = new();
        
        [Serializable]
        private class LightInstance
        {
            public Light2D light;
            [SerializeField, HideInInspector]
            private float initialIntensity;
            public float InitialIntensity => initialIntensity;

            public LightInstance(Light2D light2D)
            {
                light = light2D;
                initialIntensity = light.intensity;
            }
        }

        public void OnValidate()
        {
            threshold = Mathf.Clamp01(threshold);
            if (parent == null)
                return;
            particleSystems = parent.GetComponentsInChildren<ParticleSystem>();
            foreach (var system in particleSystems)
            {
                var main = system.main;
                main.playOnAwake = false;
            }
            
            var tempLights = parent.GetComponentsInChildren<Light2D>();
            lights = tempLights.Select(light => new LightInstance(light)).ToArray();
        }
        
        public void SetActive(bool active)
        {
            parent.gameObject.SetActive(active);
            if (active)
                foreach (var system in particleSystems)
                    system.Play();
            else
                foreach (var system in particleSystems)
                    system.Stop();

            currentlyUpdating = new List<LightInstance>();
            foreach (var instance in lights)
            {
                instance.light.intensity = 0;
                currentlyUpdating.Add(instance);
            }
        }

        public void UpdateColor(BackgroundState previous, float state)
        {
            var color = background.color;
            var t = Mathf.InverseLerp(previous.threshold, previous.nextThreshold, 1f - state);
            color.a = Mathf.Lerp(color.a, t, Time.deltaTime);
            background.color = color;
        }

        public void UpdateLights(float dt)
        {
            if (currentlyUpdating.Count <= 0) return;
            for (var i = currentlyUpdating.Count - 1; i >= 0; i--)
            {
                var instance = currentlyUpdating[i];
                instance.light.intensity = Mathf.Lerp(instance.light.intensity, instance.InitialIntensity, dt);
                if (instance.InitialIntensity - instance.light.intensity <= 0.01f)
                    currentlyUpdating.RemoveAt(i);
            }
        }
    }

    private void OnValidate()
    {
        buckets = FindObjectsByType<Bucket>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        metronome = FindFirstObjectByType<Metronome>();
        foreach (var background in backgrounds) background.OnValidate();

        for (var i = 0; i < backgrounds.Length; i++)
        {
            var nextBackground = backgrounds[i + 1 >= backgrounds.Length ? 0 : i + 1];
            var currentBackground = backgrounds[i];
            currentBackground.nextThreshold = nextBackground.threshold;
            if (currentBackground == backgrounds.Last())
                currentBackground.nextThreshold = 1;
        }
    }

    private void OnEnable()
    {
        currentAmount = 0;
        state = 1;
        lastIndex = -1;
        foreach (var bucket in buckets)
            bucket.OnEmptyBucket += OnEmptyBucket;
        metronome.OnBeat.AddListener(OnBeat);
    }

    private void OnDisable()
    {
        foreach (var bucket in buckets)
            bucket.OnEmptyBucket -= OnEmptyBucket;
        metronome.OnBeat.RemoveListener(OnBeat);
    }

    private void OnEmptyBucket(float amount) => AddToAmount(amount);

    public void AddToAmount(float amount, float multiplier = 1)
    {
        if (Judge.maxAmount <= 0 || amount <= 0)
            return;
        currentAmount += amount;
        var appliedMultiplier = Mathf.Approximately(multiplier, 1) ? multiplier : playerHitMultiplier;
        var percentage = (float)currentAmount / Judge.maxAmount;
        percentage *= appliedMultiplier;
        state = Mathf.Clamp01(1f - percentage);

        if (state <= 0)
            state = 0;
    }
    private void OnBeat() => AddToAmount(autoIncrement); // given player progression, player will always progress

    private void Update()
    {
        var index = GetCurrent(1f - state);
        var current = backgrounds[index];
        HandleActiveStates(current, index);
        
        current.UpdateLights(Time.deltaTime);
        
        if (index + 1 >= backgrounds.Length)
            return;
        var next = backgrounds[index + 1];
        next.UpdateColor(current, state);
    }

    private void HandleActiveStates(BackgroundState current, int index)
    {
        if (index == lastIndex) return;
        current.SetActive(true);
        if (lastIndex != -1)
            backgrounds[lastIndex].SetActive(false);
        lastIndex = index;
    }

    private int GetCurrent(float givenStatePoint)
    {
        var found = -1;
        for (var i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i].threshold <= givenStatePoint)
                found = i;
            else
                break;
        }
        return found;
    }
}
