using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SneakawayStudio
{
    /// <summary>
    /// The Sneakaway Studio intro ("Line draw"): on black, the triangle outline traces itself
    /// from the apex, the S rises in, sneakaway.studio types out, then everything fades to the
    /// game's colour. A tap skips straight to the fade.
    /// <para>Two ways to use it (see the package README):</para>
    /// <list type="bullet">
    /// <item>Standalone: put the prefab in a first scene and set <see cref="nextScene"/>. It plays,
    /// loads that scene behind itself, then fades away.</item>
    /// <item>From your own loader: leave <see cref="nextScene"/> empty, call
    /// <see cref="PlayAsync"/> while your game loads, then <see cref="HideAsync"/> once the
    /// first scene is ready.</item>
    /// </list>
    /// Everything is a sprite, no fonts. The triangle is one Image (Filled, Radial 360, origin
    /// Top, clockwise): a radial sweep around the outline reveals it apex → right side → base →
    /// left side, like a pen. The URL is an Image (Filled, Horizontal, origin Left) whose fill
    /// steps from one character's right edge to the next, so it types out.
    /// No tweening library: the timeline is computed each frame in <see cref="Update"/>.
    /// </summary>
    public class SneakawayIntro : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup group;

        [SerializeField]
        private Image triangle;

        [SerializeField]
        private Image letter;

        [SerializeField]
        private Image url;

        [Tooltip("Where each character of the URL image ends, as a share of its width (left to right). The fill steps through these so the URL types out. Empty: a smooth wipe instead. Update these if you replace the URL image.")]
        [SerializeField]
        private float[] urlCharEnds =
        {
            0.0513f, 0.1198f, 0.1914f, 0.2580f, 0.3236f, 0.3842f, 0.4867f, 0.5466f,
            0.6145f, 0.6441f, 0.7041f, 0.7570f, 0.8239f, 0.8998f, 0.9328f, 1f,
        };

        [Tooltip("Full-screen Image in the game's background colour; fades in at the end. Set its colour to your game's.")]
        [SerializeField]
        private Image fadeOverlay;

        [Tooltip("Tapping anywhere skips to the fade.")]
        [SerializeField]
        private Button skipButton;

        [Header("Standalone")]
        [Tooltip("If set, the intro plays by itself on Start, loads this scene (by name, in Build Profiles) behind it, then fades away. Leave empty when your own loader calls PlayAsync/HideAsync.")]
        [SerializeField]
        private string nextScene = "";

        [Header("Timing (seconds)")]
        [SerializeField]
        private float drawSeconds = 1f;

        [SerializeField]
        private float letterAt = 0.75f;

        [SerializeField]
        private float letterSeconds = 0.5f;

        [SerializeField]
        private float urlAt = 1.25f;

        [SerializeField]
        private float urlSeconds = 0.7f;

        [Tooltip("When the fade to the game's colour starts; the logo holds until then.")]
        [SerializeField]
        private float fadeAt = 2.9f;

        [SerializeField]
        private float fadeSeconds = 0.5f;

        [SerializeField]
        private float hideSeconds = 0.4f;

        [Tooltip("Letter rise, as a share of the triangle's height.")]
        [SerializeField]
        private float letterRise = 0.06f;

        [Tooltip("Editor only: play the intro in Play mode. Off skips it (standalone mode then loads the next scene at once).")]
        [SerializeField]
        private bool playInEditor = true;

        // Games load (saved data, word lists, scenes) while the intro plays, and loading can stall
        // a frame. Advancing at most 1/30 s per frame makes a stall pause the drawing for a moment
        // instead of jumping it ahead.
        const float MaxStep = 1f / 30f;

        float time;
        bool playing;
        TaskCompletionSource<bool> played;

        float hideTime;
        bool hiding;
        TaskCompletionSource<bool> hidden;

        Vector2 letterHome;
        float triangleHeight;

        /// <summary>False in the editor when <see cref="playInEditor"/> is off.</summary>
        public bool ShouldPlay => !Application.isEditor || playInEditor;

        float End => fadeAt + fadeSeconds;

        void Awake()
        {
            if (skipButton != null)
                skipButton.onClick.AddListener(Skip);
            letterHome = letter.rectTransform.anchoredPosition;
            Apply(0);
        }

        void Start()
        {
            if (!string.IsNullOrEmpty(nextScene))
                _ = RunStandaloneAsync();
        }

        async Task RunStandaloneAsync()
        {
            if (!ShouldPlay)
            {
                SceneManager.LoadScene(nextScene);
                return;
            }
            // survive the scene change, so it can fade away on top of the next scene
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            AsyncOperation load = SceneManager.LoadSceneAsync(nextScene);
            load.allowSceneActivation = false;
            await PlayAsync();
            load.allowSceneActivation = true;
            while (!load.isDone)
                await Task.Yield();
            await HideAsync();
        }

        /// <summary>Plays the intro; finishes once the screen is the game's colour.</summary>
        public Task PlayAsync()
        {
            gameObject.SetActive(true);
            triangleHeight = triangle.rectTransform.rect.height;
            group.alpha = 1;
            time = 0;
            Apply(0);
            played = new TaskCompletionSource<bool>();
            playing = true;
            return played.Task;
        }

        /// <summary>Jumps to the fade (a tap).</summary>
        public void Skip()
        {
            if (playing && time < fadeAt)
            {
                time = fadeAt;
                Apply(time);
            }
        }

        /// <summary>Fades the whole intro away (revealing the scene loaded behind it), then
        /// removes it.</summary>
        public Task HideAsync()
        {
            if (this == null)
                return Task.CompletedTask;
            // only the game-colour layer should fade: with the logo and black background still
            // under it, they'd show through as it turns transparent (a flash of the logo)
            playing = false;
            played?.TrySetResult(true);
            SetAlpha(fadeOverlay, 1);
            triangle.rectTransform.parent.gameObject.SetActive(false);
            if (skipButton != null)
                skipButton.gameObject.SetActive(false);
            hidden = new TaskCompletionSource<bool>();
            hideTime = 0;
            hiding = true;
            return hidden.Task;
        }

        /// <summary>Removes the intro at once (when it doesn't play).</summary>
        public void Remove() => Destroy(gameObject);

        void Update()
        {
            if (playing)
            {
                time = Mathf.Min(time + Mathf.Min(Time.unscaledDeltaTime, MaxStep), End);
                Apply(time);
                if (time >= End)
                {
                    playing = false;
                    played.TrySetResult(true);
                }
            }
            if (hiding)
            {
                hideTime += Time.unscaledDeltaTime;
                float p = hideSeconds > 0 ? Mathf.Clamp01(hideTime / hideSeconds) : 1;
                group.alpha = 1 - OutCubic(p);
                if (p >= 1)
                {
                    hiding = false;
                    hidden.TrySetResult(true);
                    Destroy(gameObject);
                }
            }
        }

        void OnDestroy()
        {
            // never leave a caller waiting
            played?.TrySetResult(true);
            hidden?.TrySetResult(true);
        }

        /// <summary>Sets everything as it looks <paramref name="t"/> seconds in.</summary>
        void Apply(float t)
        {
            triangle.fillAmount = Progress(t, 0, drawSeconds);

            float l = OutCubic(Progress(t, letterAt, letterSeconds));
            SetAlpha(letter, l);
            letter.rectTransform.anchoredPosition =
                letterHome + Vector2.down * ((1 - l) * triangleHeight * letterRise);

            float u = Progress(t, urlAt, urlSeconds);
            if (urlCharEnds == null || urlCharEnds.Length == 0)
                url.fillAmount = u;
            else
            {
                int shown = Mathf.RoundToInt(urlCharEnds.Length * u);
                url.fillAmount = shown > 0 ? urlCharEnds[shown - 1] : 0;
            }

            SetAlpha(fadeOverlay, InOutCubic(Progress(t, fadeAt, fadeSeconds)));
        }

        static float Progress(float t, float start, float seconds) =>
            seconds > 0 ? Mathf.Clamp01((t - start) / seconds) : (t >= start ? 1 : 0);

        static float OutCubic(float x) => 1 - Mathf.Pow(1 - x, 3);

        static float InOutCubic(float x) =>
            x < 0.5f ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;

        static void SetAlpha(Graphic g, float a)
        {
            Color c = g.color;
            c.a = a;
            g.color = c;
        }
    }
}
