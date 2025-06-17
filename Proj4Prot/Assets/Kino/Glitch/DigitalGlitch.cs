using UnityEngine;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kino Image Effects/Digital Glitch")]
    public class DigitalGlitch : MonoBehaviour
    {
        #region Public Properties

        [SerializeField, Range(0, 1)]
        float _intensity = 0;

        public float intensity {
            get { return _intensity; }
            set { _intensity = value; }
        }

        [SerializeField]
        Color _glitchColor = new Color(1, 1, 1, 1);

        public Color glitchColor {
            get { return _glitchColor; }
            set { _glitchColor = value; }
        }

        #endregion

        #region Private Properties

        [SerializeField] Shader _shader;

        // Making this internal temporarily for the editor script to access,
        // or add a public getter for it. For production, consider design.
        internal Material _material; // Changed from private for editor access
        Texture2D _noiseTexture;
        RenderTexture _trashFrame1;
        RenderTexture _trashFrame2;

        // --- NEW VARIABLES FOR SMOOTH INTENSITY CONTROL ---
        private float _lerpTargetIntensity;
        private float _lerpStartTime;
        private float _currentLerpDuration; // How long this specific transition should take
        private bool _isLerpingIntensity = false;
        private float _lerpInitialIntensity; // The intensity value when the lerp started
        // --- END NEW VARIABLES ---

        #endregion

        #region Public Methods

        // --- NEW PUBLIC METHOD TO TRIGGER THE LERP ---
        /// <summary>
        /// Smoothly changes the glitch intensity to a target value over a specified duration.
        /// </summary>
        /// <param name="targetIntensity">The desired final intensity (0-1).</param>
        /// <param name="duration">The time in seconds to reach the target intensity.</param>
        public void SetIntensitySmoothly(float targetIntensity, float duration = 0.5f) // Default duration of 0.5s
        {
            // If we're already at the target intensity, or duration is zero/negative, just set it directly and stop lerping
            if (Mathf.Approximately(this.intensity, targetIntensity) || duration <= 0)
            {
                this.intensity = targetIntensity;
                _isLerpingIntensity = false;
                return;
            }

            // Initialize lerp parameters
            _lerpInitialIntensity = this.intensity; // Start from the current intensity value
            _lerpTargetIntensity = targetIntensity;
            _currentLerpDuration = duration;
            _lerpStartTime = Time.time; // Record the time when the lerp begins
            _isLerpingIntensity = true;
        }
        // --- END NEW PUBLIC METHOD ---

        #endregion

        #region Private Functions

        static Color RandomColor()
        {
            return new Color(Random.value, Random.value, Random.value, Random.value);
        }

        void SetUpResources()
        {
            if (_material != null) return;

            _material = new Material(_shader);
            _material.hideFlags = HideFlags.DontSave;

            _noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false);
            _noiseTexture.hideFlags = HideFlags.DontSave;
            _noiseTexture.wrapMode = TextureWrapMode.Clamp;
            _noiseTexture.filterMode = FilterMode.Point;

            _trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
            _trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
            _trashFrame1.hideFlags = HideFlags.DontSave;
            _trashFrame2.hideFlags = HideFlags.DontSave;

            UpdateNoiseTexture();
        }

        void UpdateNoiseTexture()
        {
            var color = RandomColor();

            for (var y = 0; y < _noiseTexture.height; y++)
            {
                for (var x = 0; x < _noiseTexture.width; x++)
                {
                    if (Random.value > 0.89f) color = RandomColor();
                    _noiseTexture.SetPixel(x, y, color);
                }
            }

            _noiseTexture.Apply();
        }

        #endregion

        #region MonoBehaviour Functions

        void Update()
        {
            // --- HANDLE INTENSITY LERPING ---
            if (_isLerpingIntensity)
            {
                float elapsed = Time.time - _lerpStartTime;
                float t = elapsed / _currentLerpDuration; // Calculate normalized time (0 to 1)

                // Clamp01 ensures 't' stays between 0 and 1, preventing overshoot and holding the target value
                this.intensity = Mathf.Lerp(_lerpInitialIntensity, _lerpTargetIntensity, Mathf.Clamp01(t));

                // If the animation is complete (t has reached or exceeded 1.0), stop lerping
                if (t >= 1.0f)
                {
                    _isLerpingIntensity = false; // Stop the lerp
                }
            }
            // --- END INTENSITY LERPING ---


            // Original noise update logic. Consider if you want noise to update constantly
            // or only when intensity is manually set (not lerping).
            // This example updates noise based on intensity whether it's lerping or not.
            if (Random.value > Mathf.Lerp(0.9f, 0.5f, _intensity))
            {
                SetUpResources();
                UpdateNoiseTexture();
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            SetUpResources();

            var fcount = Time.frameCount;
            if (fcount % 13 == 0) Graphics.Blit(source, _trashFrame1);
            if (fcount % 73 == 0) Graphics.Blit(source, _trashFrame2);

            _material.SetFloat("_Intensity", _intensity);
            _material.SetTexture("_NoiseTex", _noiseTexture);
            var trashFrame = Random.value > 0.5f ? _trashFrame1 : _trashFrame2;
            _material.SetTexture("_TrashTex", trashFrame);

            // Debugging line (optional, you can remove it once confident)
            //Debug.Log($"Sending Glitch Color: {_glitchColor} (R:{_glitchColor.r}, G:{_glitchColor.g}, B:{_glitchColor.b}, A:{_glitchColor.a})");
            _material.SetColor("_GlitchColor", _glitchColor);

            Graphics.Blit(source, destination, _material);
        }

        void OnDisable()
        {
            if (_material != null)
            {
                DestroyImmediate(_material);
                _material = null;
            }
            if (_noiseTexture != null)
            {
                DestroyImmediate(_noiseTexture);
                _noiseTexture = null;
            }
            if (_trashFrame1 != null)
            {
                DestroyImmediate(_trashFrame1);
                _trashFrame1 = null;
            }
            if (_trashFrame2 != null)
            {
                DestroyImmediate(_trashFrame2);
                _trashFrame2 = null;
            }
        }

        #endregion
    }
}