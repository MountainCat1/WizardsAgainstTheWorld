// using System;
// using CRTFilter;
// using Managers;
// using UnityEngine;
// using UnityEngine.Rendering.Universal;
// using Zenject;
//
//
// [Serializable]
// public struct CrtSettings
// {
//     public float blur;
//     public float chromaticAberration;
//     public float pixelResolutionX;
//     public float pixelResolutionY;
//     public float gamma;
//     public float noiseSpeed;
//     public float noiseSize;
//     public float noiseAlpha;
//     public float brightness;
// }
//
// public class CrtEffectController : MonoBehaviour
// {
//     [Inject] private IPhaseManager _phaseManager;
//     [Inject] private IPlayerCharacterProvider _playerProvider;
//     
//     [SerializeField] private Renderer2DData renderer2DData;
//     [SerializeField] private CrtSettings defaultCrtSettings;
//     [SerializeField] private CrtSettings animatedCrtSettings;
//     [SerializeField] private CrtSettings deathCrtSettings;
//     [SerializeField] private Animator animator;
//     
//     private CRTRendererFeature _crtFilter;
//     
//     private static readonly int PlayerDeath = Animator.StringToHash("PlayerDeath");
//     private static readonly int End = Animator.StringToHash("End");
//
//     private void Start()
//     {
//         _crtFilter = renderer2DData.rendererFeatures.Find(x => x is CRTRendererFeature) as CRTRendererFeature;
//         if (!_crtFilter)
//         {
//             GameLogger.LogError("CRT Filter not found");
//         }
//         
//         animatedCrtSettings = defaultCrtSettings;
//         
//         _phaseManager.PhaseChanged += OnNewPhase;
//         
//         SetLayerWeight(0f);
//         
//         var player = _playerProvider.Get();
//         player.Death += OnPlayerDeath;
//     }
//
//     private void OnPlayerDeath(DeathContext obj)
//     {
//         animator.SetTrigger(PlayerDeath);
//     }
//
//     private void OnNewPhase(int phase)
//     {
//         if(_playerProvider.PlayerDead)
//             return;
//         
//         
//         if(phase == -1)
//         {
//             SetLayerWeight(0f);
//         }
//         
//         if (phase == 1)
//         {
//             SetLayerWeight(0f);
//         }
//         if(phase == 2)
//         {
//             SetLayerWeight(0f);
//         }
//         if(phase == 3)
//         {
//             SetLayerWeight(0.0f);
//         }
//         if(phase == 4)
//         {
//             SetLayerWeight(0.01f);
//         }
//         if(phase == 5)
//         {
//             SetLayerWeight(0.2f);
//         }
//         if(phase == 6)
//         {
//             SetLayerWeight(1f);
//             animator.SetTrigger(End);
//         }
//     }
//     
//
//     private void Update()
//     {
//         SetSettings(animatedCrtSettings);
//     }
//
//     private void OnDestroy()
//     {
//        SetSettings(defaultCrtSettings);
//     }
//     
//     private void SetSettings(CrtSettings settings)
//     {
//         _crtFilter.blur = settings.blur;
//         SetChromaticAberration(settings.chromaticAberration);
//         _crtFilter.pixelResolutionX = settings.pixelResolutionX;
//         _crtFilter.pixelResolutionY = settings.pixelResolutionY;
//         _crtFilter.gamma = settings.gamma;
//         _crtFilter.noiseSize = settings.noiseSize;
//         _crtFilter.noiseSpeed = settings.noiseSpeed;
//         _crtFilter.brightness = settings.brightness;
//         _crtFilter.noiseAlpha = settings.noiseAlpha;
//     }
//     
//     private void SetChromaticAberration(float value)
//     {
//         _crtFilter.chromaticAberration = value;
//         _crtFilter.redOffset = new Vector2(value / 10, value / 10);
//         _crtFilter.blueOffset = new Vector2(0, value / 10 * 1.4f);
//         _crtFilter.greenOffset = new Vector2(-value / 10, value / 10);
//     }
//     
//     private void SetLayerWeight(float weight)
//     {
//         var layers = animator.layerCount;
//         for (int i = 0; i < layers; i++)
//         {
//             animator.SetLayerWeight(i, weight);
//         }
//     }
// }
//
