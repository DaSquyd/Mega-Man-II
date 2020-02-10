using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameHandler : MonoBehaviour {
    public struct VirtualController {
        public VirtualButton Jump;
        public VirtualButton Shoot;
        public VirtualButton Left;
        public VirtualButton Right;
        public VirtualButton Up;
        public VirtualButton Down;
        public VirtualButton Start;
        public VirtualButton Select;
        public VirtualButton FrameStep;
    }

    public struct VirtualButton {
        public bool Value;
        public bool Press;
        public int IntValue {
            get {
                return Value ? 1 : 0;
            }
        }
    }

    public static VirtualController vc = new VirtualController();

    public static GameHandler handler;

    public static bool FrameStepping = false;
    public static bool Step {
        get; private set;
    }

    public static Player player;
    public static Camera camera;
    public static Image healthBarImage;

    public static AudioClip playerShootAudio;
    public static AudioClip playerHitAudio;
    public static AudioClip playerDieAudio;
    public static AudioClip enemyDieAudio;
    public static AudioClip loseMusic;
    public static AudioClip winMusic;

    public Image m_healthBarImage;

    public AudioClip m_playerShootAudio;
    public AudioClip m_playerHitAudio;
    public AudioClip m_playerDieAudio;
    public AudioClip m_enemyDieAudio;
    public AudioClip m_loseMusic;
    public AudioClip m_winMusic;

    public GameObject healthBar;
    public GameObject loseMenu;
    public GameObject winMenu;

    public bool lose;
    public int loseCount;
    public bool loseCountComplete;

    private void Awake() {
        handler = this;

        healthBarImage = m_healthBarImage;

        playerShootAudio = m_playerShootAudio;
        playerHitAudio = m_playerHitAudio;
        playerDieAudio = m_playerDieAudio;
        enemyDieAudio = m_enemyDieAudio;
        loseMusic = m_loseMusic;
        winMusic = m_winMusic;

        DontDestroyOnLoad(gameObject);

        camera = Camera.main;
    }

    public void GameStart() {
        Destroy(GameObject.Find("Main Event System"));
        healthBar.SetActive(true);
        loseMenu.SetActive(false);
        lose = false;
        loseCount = 0;
        loseCountComplete = false;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    private void FixedUpdate() {

        Controls();

        if (lose) {

            if (loseCount < 120)
                loseCount++;
            else if (!loseCountComplete) {
                PlayAudio(loseMusic);
                loseCountComplete = true;
                loseMenu.SetActive(true);
                healthBar.SetActive(false);
            }

            return;
        }

        if (player == null) {
            player = Player.player;
            return;
        }
        if (camera == null) {
            camera = Camera.main;
            return;
        }


        Step = false;

        if (!FrameStepping)
            return;
        if (vc.FrameStep.Press) {
            Step = true;
        }
    }

    private void Controls() {

        SetControlOutput(ref vc.Jump, KeyCode.X, KeyCode.K, "Jump");
        SetControlOutput(ref vc.Shoot, KeyCode.Z, KeyCode.J, "Shoot");
        SetControlOutput(ref vc.Left, KeyCode.LeftArrow, KeyCode.A, "Horizontal", true);
        SetControlOutput(ref vc.Right, KeyCode.RightArrow, KeyCode.D, "Horizontal", false);
        SetControlOutput(ref vc.Up, KeyCode.UpArrow, KeyCode.W, "Vertical", true);
        SetControlOutput(ref vc.Down, KeyCode.DownArrow, KeyCode.S, "Vertical", false);
        SetControlOutput(ref vc.Start, KeyCode.Return, KeyCode.Return, "Start");
        SetControlOutput(ref vc.Select, KeyCode.RightShift, KeyCode.RightShift, "Select");
        //SetControlOutput(ref vc.FrameStep, KeyCode.Slash, "Step");

        if (EventSystem.current != null) {
            StandaloneInputModule inputModule = EventSystem.current.GetComponent<StandaloneInputModule>();

            for (int i = 0; i < Input.GetJoystickNames().Length; i++) {
                if (Input.GetJoystickNames()[i].Contains("Xbox")) {
                    inputModule.horizontalAxis = "Horizontal_Xbox";
                    inputModule.verticalAxis = "Vertical_Xbox";
                    inputModule.submitButton = "Jump_Xbox";
                    inputModule.cancelButton = "Shoot_Xbox";
                } else if (Input.GetJoystickNames()[i].Contains("Wireless")) {
                    inputModule.horizontalAxis = "Horizontal_PS";
                    inputModule.verticalAxis = "Vertical_PS";
                    inputModule.submitButton = "Jump_PS";
                    inputModule.cancelButton = "Shoot_PS";
                } else {
                    continue;
                }
                break;
            }

            if (EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
        }
    }

    private void SetControlOutput(ref VirtualButton button, KeyCode key1, KeyCode key2, string axis, bool negative = false) {

        string newAxis = "None";
        for (int i = 0; i < Input.GetJoystickNames().Length; i++) {
            if (Input.GetJoystickNames()[i].Contains("Xbox")) {
                newAxis = axis + "_Xbox";
            } else if (Input.GetJoystickNames()[i].Contains("Wireless")) {
                newAxis = axis + "_PS";
            } else {
                continue;
            }
            break;
        }

        if (Input.GetKey(key1) || Input.GetKey(key2) || Input.GetAxisRaw(newAxis) * (negative ? -1f : 1f) > 0f) {
            if (button.Value) {
                button.Press = false;
            } else {
                button.Value = true;
                button.Press = true;
            }
        } else {
            button.Value = false;
            button.Press = false;
        }
    }

    private void LateUpdate() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Return)) {
            FrameStepping = !FrameStepping;
        }
    }

    public static void PlayAudio(AudioClip clip) {
        AudioSource source = handler.GetComponent<AudioSource>();
        source.PlayOneShot(clip);
    }

    public static void Lose() {
        camera.GetComponent<AudioSource>().Stop();
        handler.lose = true;
    }

    public static void Win() {
        camera.GetComponent<AudioSource>().Stop();
        PlayAudio(winMusic);
        handler.winMenu.SetActive(true);
        handler.healthBar.SetActive(false);
    }
}
