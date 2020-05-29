using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRTK {
    /// <summary>
    ///     The `[VRSimulator_CameraRig]` prefab is a mock Camera Rig set up that can be used to develop with VRTK without the
    ///     need for VR Hardware.
    /// </summary>
    /// <remarks>
    ///     Use the mouse and keyboard to move around both play area and hands and interacting with objects without the need of
    ///     a hmd or VR controls.
    /// </remarks>
    public class SDK_InputSimulator : MonoBehaviour {
        /// <summary>
        ///     Mouse input mode types
        /// </summary>
        public enum MouseInputMode {
            /// <summary>
            ///     Mouse movement is always treated as mouse input.
            /// </summary>
            Always,

            /// <summary>
            ///     Mouse movement is only treated as movement when a button is pressed.
            /// </summary>
            RequiresButtonPress,
			WhileLocked
		}

		#region Public fields

		[SerializeField] private VRTK_SDKSetup m_SimulatorSetup;
		[SerializeField] public Transform m_Player;

		[Header("General Settings"), Tooltip("Show control information in the upper left corner of the screen.")]
		public bool showControlHints = true;

		[Tooltip("Hide hands when disabling them.")]
		public bool hideHandsAtSwitch;

		[Tooltip("Reset hand position and rotation when enabling them.")]
		public bool resetHandsAtSwitch = true;

		[Tooltip("Displays an axis helper to show which axis the hands will be moved through.")]
		public bool showHandAxisHelpers = true;

		[Header("Mouse Cursor Lock Settings"), Tooltip("Lock the mouse cursor to the game window.")]
		public bool lockMouseToView = true;

		[Tooltip("Whether the mouse movement always acts as input or requires a button press.")]
		public MouseInputMode mouseMovementInput = MouseInputMode.Always;

		[Header("Manual Adjustment Settings"), Tooltip("Adjust hand movement speed.")]
		public float handMoveMultiplier = 0.002f;

		[Tooltip("Adjust hand rotation speed.")]
		public float handRotationMultiplier = 0.5f;

		[Tooltip("Adjust player movement speed.")]
		public float playerMoveMultiplier = 5f;

		[Tooltip("Adjust player rotation speed.")]
		public float playerRotationMultiplier = 0.5f;

		[Tooltip("Adjust player sprint speed.")]
		public float playerSprintMultiplier = 2f;

		[Tooltip("Adjust the speed of the cursor movement in locked mode.")]
		public float lockedCursorMultiplier = 5f;

		[Tooltip("The Colour of the GameObject representing the left hand.")]
		public Color leftHandColor = Color.red;

		[Tooltip("The Colour of the GameObject representing the right hand.")]
		public Color rightHandColor = Color.green;

		[Header("Operation Key Binding Settings"), Tooltip("Key used to enable mouse input if a button press is required.")]
		public KeyCode mouseMovementKey = KeyCode.Mouse1;

		[Tooltip("Key used to toggle control hints on/off.")]
		public KeyCode toggleControlHints = KeyCode.F1;

		[Tooltip("Key used to toggle control hints on/off.")]
		public KeyCode toggleMouseLock = KeyCode.F4;

		[Tooltip("Key used to switch between left and righ hand.")]
		public KeyCode changeHands = KeyCode.Tab;

		[Tooltip("Key used to switch hands On/Off.")]
		public KeyCode handsOnOff = KeyCode.LeftAlt;

		[Tooltip("Key used to switch between positional and rotational movement.")]
		public KeyCode rotationPosition = KeyCode.LeftShift;

		[Tooltip("Key used to switch between X/Y and X/Z axis.")]
		public KeyCode changeAxis = KeyCode.LeftControl;

		[Tooltip("Key used to distance pickup with left hand.")]
		public KeyCode distancePickupLeft = KeyCode.Mouse0;

		[Tooltip("Key used to distance pickup with right hand.")]
		public KeyCode distancePickupRight = KeyCode.Mouse1;

		[Tooltip("Key used to enable distance pickup.")]
		public KeyCode distancePickupModifier = KeyCode.LeftControl;

		[Header("Movement Key Binding Settings"), Tooltip("Key used to move forward.")]
		public KeyCode moveForward = KeyCode.W;

		[Tooltip("Key used to move to the left.")]
		public KeyCode moveLeft = KeyCode.A;

		[Tooltip("Key used to move backwards.")]
		public KeyCode moveBackward = KeyCode.S;

		[Tooltip("Key used to move to the right.")]
		public KeyCode moveRight = KeyCode.D;

        [Tooltip("Key used to move up")]
		public KeyCode up = KeyCode.Space;

        [Tooltip("Key used to move down")]
		public KeyCode down = KeyCode.LeftShift;


        [Header("Controller Key Binding Settings"), Tooltip("Key used to simulate trigger button.")]
		public KeyCode triggerAlias = KeyCode.Mouse1;

		[Tooltip("Key used to simulate grip button.")]
		public KeyCode gripAlias = KeyCode.Mouse0;

		[Tooltip("Key used to simulate touchpad button.")]
		public KeyCode touchpadAlias = KeyCode.Q;

		[Tooltip("Key used to simulate button one.")]
		public KeyCode buttonOneAlias = KeyCode.E;

		[Tooltip("Key used to simulate button two.")]
		public KeyCode buttonTwoAlias = KeyCode.R;

		[Tooltip("Key used to simulate start menu button.")]
		public KeyCode startMenuAlias = KeyCode.F;

		[Tooltip("Key used to switch between button touch and button press mode.")]
		public KeyCode touchModifier = KeyCode.T;

		[Tooltip("Key used to switch between hair touch mode.")]
		public KeyCode hairTouchModifier = KeyCode.H;

		#endregion

		#region Protected fields

		private bool m_IsHand;
		private GameObject m_HintCanvas;
		private Text m_HintText;
		private Transform m_CurrentHand;
		private Vector3 m_OldPos;

		private SDK_ControllerSim m_RightController;
		private SDK_ControllerSim m_LeftController;
		private GameObject m_CrossHairPanel;
		private Transform m_LeftHandHorizontalAxisGuide;
		private Transform m_LeftHandVerticalAxisGuide;
		private Transform m_RightHandHorizontalAxisGuide;
		private Transform m_RightHandVerticalAxisGuide;

		private static SDK_InputSimulator m_Instance;
		private static bool m_Destroyed;

		#endregion

		public Transform RightHand => m_SimulatorSetup.actualRightController.transform;
		public Transform LeftHand => m_SimulatorSetup.actualLeftController.transform;
		public Transform Neck => m_SimulatorSetup.actualHeadset.transform.parent;
		public bool IsHand => m_IsHand;
        public bool KeyPressedUp { get; private set; }
        public bool KeyPressedDown { get; private set; }
        public bool KeyPressedForward { get; private set; }
        public bool KeyPressedBackward { get; private set; }
        public bool KeyPressedLeft { get; private set; }
        public bool KeyPressedRight { get; private set; }
		private static bool IsCursorInLockPos {
			get {
				bool inLockPos = InMiddle(Input.mousePosition.x, Screen.width) && InMiddle(Input.mousePosition.y, Screen.height);
				// if (!inLockPos) {
				// 	Debug.Log($"Cursor is NOT in lock pos: {Input.mousePosition} vs. ({Screen.width/2:N0}, {Screen.height/2:N0})");
				// }
				return inLockPos;
			}
		}

		/// <summary>
        ///     The FindInScene method is used to find the `[VRSimulator_CameraRig]` GameObject within the current scene.
        /// </summary>
        /// <returns>
        ///     Returns the found `[VRSimulator_CameraRig]` GameObject if it is found. If it is not found then it prints a
        ///     debug log error.
        /// </returns>
        public static SDK_InputSimulator FindInScene() {
			if (m_Instance == null && !m_Destroyed) {
				m_Instance = VRTK_SharedMethods.FindEvenInactiveComponent<SDK_InputSimulator>(true);
				if (!m_Instance) { VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_SCENE, "[VRSimulator_CameraRig]", "SDK_InputSimulator", ". check that the `VRTK/Prefabs/CameraRigs/[VRSimulator_CameraRig]` prefab been added to the scene.")); }
			}
			return m_Instance;
		}

		protected virtual void Awake() { VRTK_SDKManager.AttemptAddBehaviourToToggleOnLoadedSetupChange(this); }

		protected virtual void OnEnable() {
			m_HintCanvas = transform.Find("Canvas/Control Hints").gameObject;
			m_CrossHairPanel = transform.Find("Canvas/CrosshairPanel").gameObject;
			m_HintText = m_HintCanvas.GetComponentInChildren<Text>();
			m_HintCanvas.SetActive(showControlHints);
			RightHand.gameObject.SetActive(false);
			LeftHand.gameObject.SetActive(false);
			m_LeftHandHorizontalAxisGuide = LeftHand.Find("Guides/HorizontalPlane");
			m_LeftHandVerticalAxisGuide = LeftHand.Find("Guides/VerticalPlane");
			m_RightHandHorizontalAxisGuide = RightHand.Find("Guides/HorizontalPlane");
			m_RightHandVerticalAxisGuide = RightHand.Find("Guides/VerticalPlane");
			m_CurrentHand = RightHand;
			m_OldPos = Input.mousePosition;
			SetHandColor(LeftHand, leftHandColor);
			SetHandColor(RightHand, rightHandColor);
			m_RightController = RightHand.GetComponent<SDK_ControllerSim>();
			m_LeftController = LeftHand.GetComponent<SDK_ControllerSim>();
			m_RightController.selected = true;
			m_LeftController.selected = false;
			m_Destroyed = false;

			SDK_SimController controllerSDK = VRTK_SDK_Bridge.GetControllerSDK() as SDK_SimController;
			if (controllerSDK != null) {
				Dictionary<string, KeyCode> keyMappings = new Dictionary<string, KeyCode> {
					{"Trigger", triggerAlias},
					{"Grip", gripAlias},
					{"TouchpadPress", touchpadAlias},
					{"ButtonOne", buttonOneAlias},
					{"ButtonTwo", buttonTwoAlias},
					{"StartMenu", startMenuAlias},
					{"TouchModifier", touchModifier},
					{"HairTouchModifier", hairTouchModifier}
				};
				controllerSDK.SetKeyMappings(keyMappings);
			}
			RightHand.gameObject.SetActive(true);
			LeftHand.gameObject.SetActive(true);
			m_CrossHairPanel.SetActive(false);
		}

		protected virtual void OnDestroy() {
			VRTK_SDKManager.AttemptRemoveBehaviourToToggleOnLoadedSetupChange(this);
			m_Destroyed = true;
		}

		protected virtual void Update() {
			if (Input.GetKeyDown(toggleControlHints)) {
				showControlHints = !showControlHints;
				m_HintCanvas.SetActive(showControlHints);
			}

			if (Input.GetKeyDown(toggleMouseLock)) { lockMouseToView = !lockMouseToView; }

			if (mouseMovementInput == MouseInputMode.RequiresButtonPress) {
				if (lockMouseToView) { Cursor.lockState = Input.GetKey(mouseMovementKey) ? CursorLockMode.Locked : CursorLockMode.None; }
				else if (Input.GetKeyDown(mouseMovementKey)) { m_OldPos = Input.mousePosition; }
			}
			else { Cursor.lockState = lockMouseToView ? CursorLockMode.Locked : CursorLockMode.None; }

			if (Input.GetKeyDown(handsOnOff)) {
				if (m_IsHand) {
					SetMove();
					ToggleGuidePlanes(false, false);
				}
				else { SetHand(); }
			}

			if (Input.GetKeyDown(changeHands)) {
				if (m_CurrentHand.name == "LeftHand") {
					m_CurrentHand = RightHand;
					m_RightController.selected = true;
					m_LeftController.selected = false;
				}
				else {
					m_CurrentHand = LeftHand;
					m_RightController.selected = false;
					m_LeftController.selected = true;
				}
			}

			if (m_IsHand) { UpdateHands(); }
			else {
				UpdateRotation();
				if (Input.GetKeyDown(distancePickupRight) && Input.GetKey(distancePickupModifier)) { TryPickup(true); }
				else if (Input.GetKeyDown(distancePickupLeft) && Input.GetKey(distancePickupModifier)) { TryPickup(false); }

				if (Input.GetKeyDown(distancePickupModifier)) { m_CrossHairPanel.SetActive(true); }
				else if (Input.GetKeyUp(distancePickupModifier)) { m_CrossHairPanel.SetActive(false); }
			}

			UpdatePosition();

			if (showControlHints) { UpdateHints(); }
		}

		protected virtual void SetHandColor(Transform hand, Color givenColor) {
			Transform foundHand = hand.Find("Hand");
			if (foundHand != null && givenColor != Color.clear) {
				Renderer[] renderers = foundHand.GetComponentsInChildren<Renderer>(true);
				for (int i = 0; i < renderers.Length; i++) { renderers[i].material.color = givenColor; }
			}
		}

		protected virtual void TryPickup(bool rightHand) {
			Ray screenRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
			RaycastHit hit;
			if (Physics.Raycast(screenRay, out hit)) {
				VRTK_InteractableObject io = hit.collider.gameObject.GetComponent<VRTK_InteractableObject>();
				if (io != null) {
					GameObject hand;
					if (rightHand) { hand = VRTK_DeviceFinder.GetControllerRightHand(); }
					else { hand = VRTK_DeviceFinder.GetControllerLeftHand(); }
					VRTK_InteractGrab grab = hand.GetComponent<VRTK_InteractGrab>();
					if (grab.GetGrabbedObject() == null) {
						hand.GetComponent<VRTK_InteractTouch>().ForceTouch(hit.collider.gameObject);
						grab.AttemptGrab();
					}
				}
			}
		}

		protected virtual void UpdateHands() {
			Vector3 mouseDiff = GetMouseDelta();

			if (IsAcceptingMouseInput()) {
				if (!Input.GetKey(changeAxis)) {
					ToggleGuidePlanes(false, true);
					if (Input.GetKey(rotationPosition)) {
						Vector3 rot = Vector3.zero;
						rot.z += (mouseDiff * handRotationMultiplier).x;
						rot.x += (mouseDiff * handRotationMultiplier).y;
						m_CurrentHand.transform.Rotate(rot * Time.deltaTime);
					}
					else {
						Vector3 pos = Vector3.zero;
						pos += mouseDiff * handMoveMultiplier;
						m_CurrentHand.transform.Translate(pos * Time.deltaTime);
					}
				}
				else {
					ToggleGuidePlanes(true, false);
					if (Input.GetKey(rotationPosition)) {
						Vector3 rot = Vector3.zero;
						rot.y += (mouseDiff * handRotationMultiplier).x;
						rot.x += (mouseDiff * handRotationMultiplier).y;
						m_CurrentHand.transform.Rotate(rot * Time.deltaTime);
					}
					else {
						Vector3 pos = Vector3.zero;
						pos.x += (mouseDiff * handMoveMultiplier).x;
						pos.z += (mouseDiff * handMoveMultiplier).y;
						m_CurrentHand.transform.Translate(pos * Time.deltaTime);
					}
				}
			}
		}

		protected virtual void UpdateRotation() {
			Vector3 mouseDiff = GetMouseDelta();

			if (IsAcceptingMouseInput()) {
				Vector3 rot = m_Player.localRotation.eulerAngles;
				rot.y += (mouseDiff * playerRotationMultiplier).x;
				m_Player.localRotation = Quaternion.Euler(rot);

				rot = Neck.rotation.eulerAngles;

				if (rot.x > 180) { rot.x -= 360; }

				if (rot.x < 80 && rot.x > -80) {
					rot.x += (mouseDiff * playerRotationMultiplier).y * -1;
					rot.x = Mathf.Clamp(rot.x, -79, 79);
					Neck.rotation = Quaternion.Euler(rot);
				}
			}
		}

		protected virtual void UpdatePosition() {
            float moveMod = Time.deltaTime * playerMoveMultiplier;
            //rewrote this to be controled within the PointAtCrosshair class
        
            KeyPressedUp = Input.GetKey(up);
            KeyPressedDown = Input.GetKey(down);
			KeyPressedForward = Input.GetKey(moveForward);
			KeyPressedBackward = Input.GetKey(moveBackward);
			KeyPressedLeft = Input.GetKey(moveLeft);
			KeyPressedRight = Input.GetKey(moveRight);
		}

		protected virtual void SetHand() {
			Cursor.visible = false;
			m_IsHand = true;
			RightHand.gameObject.SetActive(true);
			LeftHand.gameObject.SetActive(true);
			m_OldPos = Input.mousePosition;
			if (resetHandsAtSwitch) {
				RightHand.transform.localPosition = new Vector3(0.2f, 1.2f, 0.5f);
				RightHand.transform.localRotation = Quaternion.identity;
				LeftHand.transform.localPosition = new Vector3(-0.2f, 1.2f, 0.5f);
				LeftHand.transform.localRotation = Quaternion.identity;
			}
		}

		protected virtual void SetMove() {
			Cursor.visible = true;
			m_IsHand = false;
			if (hideHandsAtSwitch) {
				RightHand.gameObject.SetActive(false);
				LeftHand.gameObject.SetActive(false);
			}
		}

		protected virtual void UpdateHints() {
			string hints = "";

			string Key(KeyCode k) => $"<b>{k}</b>";

			string mouseInputRequires = "";
			if (mouseMovementInput == MouseInputMode.RequiresButtonPress) { mouseInputRequires = $" ({Key(mouseMovementKey)})"; }

			// WASD Movement
			hints += $"Toggle Control Hints: {Key(toggleControlHints)}\n\n";
			hints += $"Toggle Mouse Lock: {Key(toggleMouseLock)}\n";
			hints += $"Move Player: <b>{moveForward}{moveLeft}{moveBackward}{moveRight}</b>\n";
			hints += $"Move Up/Down: {Key(up)}/{Key(down)}\n\n";

			if (m_IsHand) {
				// Controllers
				if (Input.GetKey(rotationPosition)) { hints += $"Mouse: <b>Controller Rotation{mouseInputRequires}</b>\n"; }
				else { hints += $"Mouse: <b>Controller Position{mouseInputRequires}</b>\n"; }
				hints += $"Modes: HMD ({Key(handsOnOff)}), Rotation ({Key(rotationPosition)})\n";

				hints += $"Controller Hand: {m_CurrentHand.name.Replace("Hand", "")} ({Key(changeHands)})\n";

				string axis = Input.GetKey(changeAxis) ? "X/Y" : "X/Z";
				hints += $"Axis: {axis} ({Key(changeAxis)})\n";

				// Controller Buttons
				string pressMode = "Press";
				if (Input.GetKey(hairTouchModifier)) { pressMode = "Hair Touch"; }
				else if (Input.GetKey(touchModifier)) { pressMode = "Touch"; }

				hints += $"\nButton Press Mode Modifiers: Touch ({Key(touchModifier)}), Hair Touch ({Key(hairTouchModifier)})\n";

				hints += $"Trigger {pressMode}: {Key(triggerAlias)}\n";
				hints += $"Grip {pressMode}: {Key(gripAlias)}\n";
				if (!Input.GetKey(hairTouchModifier)) {
					hints += $"Touchpad {pressMode}: {Key(touchpadAlias)}\n";
					hints += $"Button One {pressMode}: {Key(buttonOneAlias)}\n";
					hints += $"Button Two {pressMode}: {Key(buttonTwoAlias)}\n";
					hints += $"Start Menu {pressMode}: {Key(startMenuAlias)}\n";
				}
			}
			else {
				// HMD Input
				hints += $"Mouse: <b>HMD Rotation{mouseInputRequires}</b>\n";
				hints += $"Modes: Controller ({Key(handsOnOff)})\n";
				hints += $"Distance Pickup Modifier: ({Key(distancePickupModifier)})\n";
				hints += $"Distance Pickup Left Hand: ({Key(distancePickupLeft)})\n";
				hints += $"Distance Pickup Right Hand: ({Key(distancePickupRight)})\n";
			}

			m_HintText.text = hints.TrimEnd();
		}

		private static bool InMiddle(float value, float range) {
			return Mathf.Abs(value - range/2) < 100;
		}

		protected virtual bool IsAcceptingMouseInput() {
			return mouseMovementInput == MouseInputMode.Always || 
					Input.GetKey(mouseMovementKey) || 
					mouseMovementInput == MouseInputMode.WhileLocked && Cursor.lockState == CursorLockMode.Locked && IsCursorInLockPos;
		}

		protected virtual Vector3 GetMouseDelta() {
			if (Cursor.lockState == CursorLockMode.Locked) { return new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * lockedCursorMultiplier; }
			Vector3 mouseDiff = Input.mousePosition - m_OldPos;
			m_OldPos = Input.mousePosition;
			return mouseDiff;
		}

		protected virtual void ToggleGuidePlanes(bool horizontalState, bool verticalState) {
			if (!showHandAxisHelpers) {
				horizontalState = false;
				verticalState = false;
			}

			if (m_LeftHandHorizontalAxisGuide != null) { m_LeftHandHorizontalAxisGuide.gameObject.SetActive(horizontalState); }

			if (m_LeftHandVerticalAxisGuide != null) { m_LeftHandVerticalAxisGuide.gameObject.SetActive(verticalState); }

			if (m_RightHandHorizontalAxisGuide != null) { m_RightHandHorizontalAxisGuide.gameObject.SetActive(horizontalState); }

			if (m_RightHandVerticalAxisGuide != null) { m_RightHandVerticalAxisGuide.gameObject.SetActive(verticalState); }
		}
	}
}