using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GreatArcStudios
{
    public class PauseManager : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainPanel;
        public GameObject vidPanel;
        public GameObject audioPanel;
        public GameObject TitleTexts;
        public GameObject mask;

        [Header("Animators")]
        public Animator audioPanelAnimator;
        public Animator vidPanelAnimator;
        public Animator quitPanelAnimator;

        [Header("UI Elements")]
        public Text pauseMenu; 
        public Dropdown aaCombo;
        public Dropdown afCombo;
        public Slider fovSlider, modelQualSlider, terrainQualSlider, highQualTreeSlider, renderDistSlider;
        public Slider terrainDensitySlider, shadowDistSlider, audioMasterSlider, audioMusicSlider;
        public Slider audioEffectsSlider, masterTexSlider, shadowCascadesSlider;
        public Toggle vSyncToggle, aoToggle, dofToggle, fullscreenToggle;
        public Text presetLabel, resolutionLabel; 

        [Header("Settings & References")]
        public string mainMenu;
        public string DOFScriptName;
        public string AOScriptName;
        public Camera mainCam;
        public GameObject mainCamObj;
        public EventSystem uiEventSystem;
        public GameObject defualtSelectedVideo, defualtSelectedAudio, defualtSelectedMain;
        public GameObject[] otherUIElements;
        public AudioSource[] music;
        public AudioSource[] effects;

        [Header("Game Configuration")]
        public float timeScale = 1f;
        public Terrain terrain;
        public Terrain simpleTerrain;
        public bool useSimpleTerrain;
        public bool hardCodeSomeVideoSettings;
        public float[] LODBias;
        public float[] shadowDist;

        [Header("Blur Effect Settings")]
        public Volume globalVolume;
        private DepthOfField dofComponent;

        // Internal states
        internal static Camera mainCamShared;
        internal static float shadowDistINI, renderDistINI, aaQualINI, densityINI, treeMeshAmtINI, fovINI;
        internal static int msaaINI, vsyncINI, lastTexLimit, lastShadowCascade;
        internal static float lastMusicMult, lastAudioMult, beforeMaster;
        internal static Resolution currentRes;

        public static bool aoBool, dofBool;
        public static Terrain readTerrain, readSimpleTerrain;
        public static bool readUseSimpleTerrain;

        private float[] _baseMusicVolumes;
        private float[] _baseEffectVolumes;
        private float[] _beforeEffectVol;
        private float _beforeMusic;

        private int _currentLevel;
        private Resolution[] allRes;
        private string[] presets;
        private bool isFullscreen, lastAOBool, lastDOFBool;
        private Resolution beforeRes;
        private SaveSettings saveSettings = new SaveSettings();

        // Biến mới theo dõi vị trí độ phân giải hiện tại
        private int _currentResIndex = 0;
        private const float ANIM_FALLBACK_DURATION = 0.3f;

        public void Start()
        {
            SharpenAllTexts();
            SetupBlurEffect();

            if (mainCam == null) mainCam = Camera.main;
            if (mainCam != null) mainCamShared = mainCam;

            readUseSimpleTerrain = useSimpleTerrain;
            if (useSimpleTerrain) readSimpleTerrain = simpleTerrain;
            else readTerrain = terrain;

            if (music != null)
            {
                _baseMusicVolumes = new float[music.Length];
                for (int i = 0; i < music.Length; i++)
                    _baseMusicVolumes[i] = music[i] != null ? music[i].volume : 1f;
            }

            if (effects != null)
            {
                _baseEffectVolumes = new float[effects.Length];
                _beforeEffectVol = new float[effects.Length];
                for (int i = 0; i < effects.Length; i++)
                {
                    _baseEffectVolumes[i] = effects[i] != null ? effects[i].volume : 1f;
                    _beforeEffectVol[i] = _baseEffectVolumes[i];
                }
            }

            lastMusicMult = audioMusicSlider != null ? audioMusicSlider.value : 1f;
            lastAudioMult = audioEffectsSlider != null ? audioEffectsSlider.value : 1f;

            if (uiEventSystem != null) uiEventSystem.firstSelectedGameObject = defualtSelectedMain;

            presets = QualitySettings.names;
            _currentLevel = QualitySettings.GetQualityLevel();
            if (presetLabel != null) presetLabel.text = presets[_currentLevel];

            // TÌM VÀ GÁN ĐỘ PHÂN GIẢI HIỆN TẠI
            allRes = Screen.resolutions;
            currentRes = Screen.currentResolution;
            beforeRes = currentRes;
            
            for (int i = 0; i < allRes.Length; i++)
            {
                if (allRes[i].width == currentRes.width && allRes[i].height == currentRes.height)
                {
                    _currentResIndex = i;
                    break;
                }
            }

            if (resolutionLabel != null) resolutionLabel.text = $"{currentRes.width} x {currentRes.height}";
            isFullscreen = Screen.fullScreen;

            lastAOBool = aoToggle != null && aoToggle.isOn;
            lastDOFBool = dofToggle != null && dofToggle.isOn;
            beforeMaster = AudioListener.volume;

            _beforeMusic = (_baseMusicVolumes != null && _baseMusicVolumes.Length > 0)
                ? _baseMusicVolumes[0]
                : 1f;

            if (mainCam != null)
            {
                renderDistINI = mainCam.farClipPlane;
                fovINI = mainCam.fieldOfView;
            }

            shadowDistINI = QualitySettings.shadowDistance;
            msaaINI = QualitySettings.antiAliasing;
            aaQualINI = QualitySettings.antiAliasing;
            vsyncINI = QualitySettings.vSyncCount;
            lastTexLimit = QualitySettings.globalTextureMipmapLimit;
            lastShadowCascade = QualitySettings.shadowCascades;

            if (TitleTexts) TitleTexts.SetActive(true);
            if (Terrain.activeTerrain != null) terrain = Terrain.activeTerrain;
            if (terrain != null) densityINI = terrain.detailObjectDensity;

            if (mainPanel) mainPanel.SetActive(false);
            if (vidPanel) vidPanel.SetActive(false);
            if (audioPanel) audioPanel.SetActive(false);
            if (mask) mask.SetActive(false);

            string savePath = Application.persistentDataPath + "/" + saveSettings.fileName;
            if (File.Exists(savePath))
                saveSettings.LoadGameSettings(File.ReadAllText(savePath));
            else
            {
                saveSettings.SaveGameSettings();
            }
        }

        private void SetupBlurEffect()
        {
            if (globalVolume != null && globalVolume.profile != null)
            {
                if (!globalVolume.profile.TryGet(out dofComponent))
                {
                    Debug.LogWarning("Không tìm thấy hiệu ứng Depth Of Field trong Global Volume. Hãy thêm nó vào Profile.");
                }
                else
                {
                    dofComponent.active = false; 
                }
            }
        }

        private void ToggleBlur(bool isPaused)
        {
            if (dofComponent != null)
            {
                dofComponent.active = isPaused;
                if (isPaused)
                {
                    dofComponent.mode.Override(DepthOfFieldMode.Gaussian);
                    dofComponent.gaussianMaxRadius.Override(1.5f);
                }
            }
        }

        private void SharpenAllTexts()
        {
            Text[] allTexts = GetComponentsInChildren<Text>(true);
            
            foreach (Text txt in allTexts)
            {
                if (txt != null && txt.fontSize > 0 && txt.fontSize < 60)
                {
                    txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txt.verticalOverflow = VerticalWrapMode.Overflow;
                    txt.fontSize *= 4;
                    txt.transform.localScale /= 4f;
                }
            }
        }

        public void Update()
        {
            readUseSimpleTerrain = useSimpleTerrain;

            if (vidPanel.activeSelf && pauseMenu != null) pauseMenu.text = "Video Menu";
            else if (audioPanel.activeSelf && pauseMenu != null) pauseMenu.text = "Audio Menu";
            else if (mainPanel.activeSelf && pauseMenu != null) pauseMenu.text = "Pause Menu";

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!mainPanel.activeSelf) OpenPauseMenu();
                else Resume();
            }
        }

        private void OpenPauseMenu()
        {
            if (uiEventSystem) uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
            mainPanel.SetActive(true);
            vidPanel.SetActive(false);
            audioPanel.SetActive(false);
            if (TitleTexts) TitleTexts.SetActive(true);
            if (mask) mask.SetActive(true);

            Time.timeScale = 0;
            ToggleOtherUI(false);
            ToggleBlur(true);
            
            // TẮT ÂM THANH KHI PAUSE
            AudioListener.pause = true; 
        }

        public void Resume()
        {
            Time.timeScale = timeScale;
            mainPanel.SetActive(false);
            vidPanel.SetActive(false);
            audioPanel.SetActive(false);
            if (TitleTexts) TitleTexts.SetActive(false);
            if (mask) mask.SetActive(false);
            ToggleOtherUI(true);
            ToggleBlur(false);
            
            // BẬT LẠI ÂM THANH KHI RESUME
            AudioListener.pause = false;
        }

        private void ToggleOtherUI(bool state)
        {
            if (otherUIElements == null) return;
            foreach (var ui in otherUIElements)
                if (ui != null) ui.SetActive(state);
        }

        public void returnToMenu()
        {
            Time.timeScale = timeScale;
            if (!string.IsNullOrEmpty(mainMenu)) SceneManager.LoadScene(mainMenu);
        }

        public void quitOptions()
        {
            vidPanel.SetActive(false);
            audioPanel.SetActive(false);
            if (quitPanelAnimator != null)
            {
                quitPanelAnimator.gameObject.SetActive(true);
                quitPanelAnimator.enabled = true;
                quitPanelAnimator.Play("QuitPanelIn");
            }
        }

        public void quitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void quitCancel()
        {
            if (quitPanelAnimator != null) 
            {
                quitPanelAnimator.gameObject.SetActive(true);
                quitPanelAnimator.Play("QuitPanelOut");
            }
        }

        public void Audio()
        {
            mainPanel.SetActive(false);
            vidPanel.SetActive(false);
            audioPanel.SetActive(true);
            if (audioPanelAnimator != null) audioPanelAnimator.enabled = true;
            audioIn();
            if (pauseMenu != null) pauseMenu.text = "Audio Menu";
        }

        public void audioIn()
        {
            if (uiEventSystem) uiEventSystem.SetSelectedGameObject(defualtSelectedAudio);
            if (audioPanelAnimator != null) audioPanelAnimator.Play("Audio Panel In");

            beforeMaster = AudioListener.volume;

            if (music != null && _baseMusicVolumes != null)
                _beforeMusic = _baseMusicVolumes[0];

            if (effects != null && _beforeEffectVol != null)
            {
                for (int i = 0; i < effects.Length; i++)
                    _beforeEffectVol[i] = effects[i] != null ? effects[i].volume : 0f;
            }

            if (audioMasterSlider != null) audioMasterSlider.value = AudioListener.volume;
            if (audioMusicSlider != null) audioMusicSlider.value = lastMusicMult;
            if (audioEffectsSlider != null) audioEffectsSlider.value = lastAudioMult;
        }

        public void updateMasterVol(float f) { AudioListener.volume = f; }

        public void updateMusicVol(float f)
        {
            if (music == null || _baseMusicVolumes == null) return;
            for (int i = 0; i < music.Length; i++)
            {
                if (music[i] != null) music[i].volume = _baseMusicVolumes[i] * f;
            }
        }

        public void updateEffectsVol(float f)
        {
            if (effects == null || _baseEffectVolumes == null) return;
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] != null) effects[i].volume = _baseEffectVolumes[i] * f;
            }
        }

        public void applyAudio()
        {
            StartCoroutine(applyAudioMain());
            if (uiEventSystem) uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
        }

        internal IEnumerator applyAudioMain()
        {
            if (audioPanelAnimator != null) audioPanelAnimator.Play("Audio Panel Out");
            yield return null;
            float duration = GetAnimatorStateDuration(audioPanelAnimator, ANIM_FALLBACK_DURATION);
            yield return new WaitForSecondsRealtime(duration);

            mainPanel.SetActive(true);
            audioPanel.SetActive(false);

            beforeMaster = AudioListener.volume;
            if (audioMusicSlider != null) lastMusicMult = audioMusicSlider.value;
            if (audioEffectsSlider != null) lastAudioMult = audioEffectsSlider.value;

            if (music != null && _baseMusicVolumes != null)
            {
                for (int i = 0; i < music.Length; i++)
                    if (music[i] != null) _baseMusicVolumes[i] = music[i].volume;
            }
            if (effects != null && _baseEffectVolumes != null)
            {
                for (int i = 0; i < effects.Length; i++)
                    if (effects[i] != null) _baseEffectVolumes[i] = effects[i].volume;
            }

            saveSettings.SaveGameSettings();
        }

        public void cancelAudio()
        {
            if (uiEventSystem) uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
            StartCoroutine(cancelAudioMain());
        }

        internal IEnumerator cancelAudioMain()
        {
            if (audioPanelAnimator != null) audioPanelAnimator.Play("Audio Panel Out");
            yield return null;
            float duration = GetAnimatorStateDuration(audioPanelAnimator, ANIM_FALLBACK_DURATION);
            yield return new WaitForSecondsRealtime(duration);

            mainPanel.SetActive(true);
            audioPanel.SetActive(false);

            AudioListener.volume = beforeMaster;

            if (effects != null && _beforeEffectVol != null)
            {
                for (int i = 0; i < effects.Length; i++)
                    if (effects[i] != null) effects[i].volume = _beforeEffectVol[i];
            }

            if (music != null && _baseMusicVolumes != null)
            {
                for (int i = 0; i < music.Length; i++)
                    if (music[i] != null) music[i].volume = _beforeMusic;
            }
        }

        public void Video()
        {
            mainPanel.SetActive(false);
            vidPanel.SetActive(true);
            audioPanel.SetActive(false);
            if (vidPanelAnimator != null) vidPanelAnimator.enabled = true;
            videoIn();
            if (pauseMenu != null) pauseMenu.text = "Video Menu";
        }

        public void videoIn()
        {
            if (uiEventSystem) uiEventSystem.SetSelectedGameObject(defualtSelectedVideo);
            if (vidPanelAnimator != null) vidPanelAnimator.Play("Video Panel In");

            int aa = QualitySettings.antiAliasing;
            if (aaCombo != null) aaCombo.value = aa == 8 ? 3 : (aa == 4 ? 2 : (aa == 2 ? 1 : 0));

            var af = QualitySettings.anisotropicFiltering;
            if (afCombo != null) afCombo.value = af == AnisotropicFiltering.ForceEnable ? 1 : (af == AnisotropicFiltering.Enable ? 2 : 0);

            if (presetLabel != null) presetLabel.text = presets[QualitySettings.GetQualityLevel()];
            if (mainCam != null)
            {
                if (fovSlider != null) fovSlider.value = mainCam.fieldOfView;
                if (renderDistSlider != null) renderDistSlider.value = mainCam.farClipPlane;
            }

            if (modelQualSlider != null) modelQualSlider.value = QualitySettings.lodBias;
            if (shadowDistSlider != null) shadowDistSlider.value = QualitySettings.shadowDistance;
            if (masterTexSlider != null) masterTexSlider.value = QualitySettings.globalTextureMipmapLimit;
            if (shadowCascadesSlider != null) shadowCascadesSlider.value = QualitySettings.shadowCascades;
            if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
            if (aoToggle != null) aoToggle.isOn = aoBool;
            if (dofToggle != null) dofToggle.isOn = dofBool;
            if (vSyncToggle != null) vSyncToggle.isOn = QualitySettings.vSyncCount == 1;

            Terrain t = useSimpleTerrain ? simpleTerrain : terrain;
            if (t != null)
            {
                if (highQualTreeSlider != null) highQualTreeSlider.value = t.treeMaximumFullLODCount;
                if (terrainDensitySlider != null) terrainDensitySlider.value = t.detailObjectDensity;
                if (terrainQualSlider != null) terrainQualSlider.value = t.heightmapMaximumLOD;
            }
        }

        public void apply()
        {
            StartCoroutine(applyVideo());
            if (uiEventSystem) uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
        }

        internal IEnumerator applyVideo()
        {
            if (vidPanelAnimator != null) vidPanelAnimator.Play("Video Panel Out");
            yield return null;
            float duration = GetAnimatorStateDuration(vidPanelAnimator, ANIM_FALLBACK_DURATION);
            yield return new WaitForSecondsRealtime(duration);

            mainPanel.SetActive(true);
            vidPanel.SetActive(false);

            if (mainCam != null)
            {
                renderDistINI = mainCam.farClipPlane;
                fovINI = mainCam.fieldOfView;
            }
            shadowDistINI = QualitySettings.shadowDistance;
            if (aoToggle != null) aoBool = aoToggle.isOn;
            if (dofToggle != null) dofBool = dofToggle.isOn;
            lastAOBool = aoBool;
            lastDOFBool = dofBool;
            beforeRes = currentRes;
            currentRes = Screen.currentResolution;
            lastTexLimit = QualitySettings.globalTextureMipmapLimit;
            lastShadowCascade = QualitySettings.shadowCascades;
            vsyncINI = QualitySettings.vSyncCount;
            isFullscreen = Screen.fullScreen;

            Terrain t = useSimpleTerrain ? simpleTerrain : terrain;
            if (t != null)
            {
                densityINI = t.detailObjectDensity;
                treeMeshAmtINI = t.treeMaximumFullLODCount;
            }

            saveSettings.SaveGameSettings();
        }

        public void cancelVideo()
        {
            if (uiEventSystem) uiEventSystem.SetSelectedGameObject(defualtSelectedMain);
            StartCoroutine(cancelVideoMain());
        }

        internal IEnumerator cancelVideoMain()
        {
            if (vidPanelAnimator != null) vidPanelAnimator.Play("Video Panel Out");
            yield return null;
            float duration = GetAnimatorStateDuration(vidPanelAnimator, ANIM_FALLBACK_DURATION);
            yield return new WaitForSecondsRealtime(duration);

            if (mainCam != null)
            {
                mainCam.farClipPlane = renderDistINI;
                mainCam.fieldOfView = fovINI;
            }
            if (Terrain.activeTerrain != null)
                Terrain.activeTerrain.detailObjectDensity = densityINI;

            mainPanel.SetActive(true);
            vidPanel.SetActive(false);

            aoBool = lastAOBool;
            dofBool = lastDOFBool;
            Screen.SetResolution(beforeRes.width, beforeRes.height, isFullscreen);
            QualitySettings.shadowDistance = shadowDistINI;
            QualitySettings.antiAliasing = msaaINI;
            QualitySettings.vSyncCount = vsyncINI;
            QualitySettings.globalTextureMipmapLimit = lastTexLimit;
            QualitySettings.shadowCascades = lastShadowCascade;
            Screen.fullScreen = isFullscreen;
        }

        public void toggleVSync(bool b) { QualitySettings.vSyncCount = b ? 1 : 0; }
        public void lodBias(float LoDBias) { QualitySettings.lodBias = LoDBias / 2.15f; }
        public void updateTex(float qual) { QualitySettings.globalTextureMipmapLimit = Mathf.RoundToInt(qual); }
        public void updateShadowDistance(float dist) { QualitySettings.shadowDistance = dist; }

        public void updateRenderDist(float f)
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam != null) mainCam.farClipPlane = f;
        }

        public void updateFOV(float fov)
        {
            if (mainCam != null) mainCam.fieldOfView = fov;
        }

        public void setFullScreen(bool b)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, b);
        }

        public void nextPreset() { ChangeQualityLevel(true); }
        public void lastPreset() { ChangeQualityLevel(false); }

        private void ChangeQualityLevel(bool increase)
        {
            if (increase) QualitySettings.IncreaseLevel();
            else QualitySettings.DecreaseLevel();

            _currentLevel = QualitySettings.GetQualityLevel();
            if (presetLabel) presetLabel.text = presets[_currentLevel];

            if (hardCodeSomeVideoSettings && _currentLevel < shadowDist.Length && _currentLevel < LODBias.Length)
            {
                QualitySettings.shadowDistance = shadowDist[_currentLevel];
                QualitySettings.lodBias = LODBias[_currentLevel];
            }
        }

        private float GetAnimatorStateDuration(Animator anim, float fallback)
        {
            if (anim == null || !anim.isActiveAndEnabled) return fallback;
            float len = anim.GetCurrentAnimatorStateInfo(0).length;
            return len > 0f ? len : fallback;
        }

        // --- CÁC HÀM XỬ LÝ ĐỘ PHÂN GIẢI ---
        public void NextRes()
        {
            if (allRes == null || allRes.Length == 0) return;
            
            _currentResIndex++;
            if (_currentResIndex >= allRes.Length) _currentResIndex = 0; 

            ApplySelectedResolution();
        }

        public void LastRes()
        {
            if (allRes == null || allRes.Length == 0) return;

            _currentResIndex--; 
            if (_currentResIndex < 0) _currentResIndex = allRes.Length - 1; 

            ApplySelectedResolution();
        }

        private void ApplySelectedResolution()
        {
            beforeRes = currentRes; 
            currentRes = allRes[_currentResIndex];
            
            if (resolutionLabel != null) 
                resolutionLabel.text = $"{currentRes.width} x {currentRes.height}";
            
            Screen.SetResolution(currentRes.width, currentRes.height, Screen.fullScreen);
        }
    }
}