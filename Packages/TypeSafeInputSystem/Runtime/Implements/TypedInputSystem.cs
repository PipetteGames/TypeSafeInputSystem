using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PipetteGames.TypeSafeInputSystem.Interfaces;

namespace PipetteGames.TypeSafeInputSystem.Implements
{
    public class TypedInputSystem<T> : ITypedInputSystem<T> where T : Enum
    {
        private InputActionAsset _inputActionAsset;
        private Dictionary<T, InputAction> _actions;

        // コールバック管理用 (Key: (アクション識別キー, 登録されたコールバック), Value: ラップされたコールバック)
        private Dictionary<(T, Action<InputAction.CallbackContext>), Action<InputAction.CallbackContext>> _startedCallbacks;
        private Dictionary<(T, Action<InputAction.CallbackContext>), Action<InputAction.CallbackContext>> _performedCallbacks;
        private Dictionary<(T, Action<InputAction.CallbackContext>), Action<InputAction.CallbackContext>> _canceledCallbacks;

        private bool _isEnabled;

        public TypedInputSystem(InputActionAsset inputActionAsset)
        {
            if (inputActionAsset == null)
            {
                throw new ArgumentNullException(nameof(inputActionAsset));
            }
            _inputActionAsset = inputActionAsset;
            _actions = new();
            _startedCallbacks = new();
            _performedCallbacks = new();
            _canceledCallbacks = new();
            _isEnabled = true;
        }

        public void RegisterAction(string actionMapName, T key)
        {
            RegisterAction(actionMapName, key, key.ToString());
        }

        public void RegisterAction(string actionMapName, T key, string actionName)
        {
            if (_actions.ContainsKey(key))
            {
                Debug.LogError($"Failed to Register action. Action for key '{key}' is already registered.");
                return;
            }

            var actionMap = _inputActionAsset.FindActionMap(actionMapName);
            if (actionMap == null)
            {
                Debug.LogError($"Failed to Register action. Action map '{actionMapName}' not found.");
                return;
            }

            var action = actionMap.FindAction(actionName);
            if (action == null)
            {
                Debug.LogError($"Failed to Register action. Action '{actionName}' not found in action map '{actionMapName}'.");
                return;
            }

            _actions[key] = action;
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        public void Enable(T action)
        {
            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to Enable action. Action for key '{action}' is not registered.");
                return;
            }

            inputAction.Enable();
        }

        public void Disable(T action)
        {
            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to Disable action. Action for key '{action}' is not registered.");
                return;
            }

            inputAction.Disable();
        }

        public bool IsEnabled()
        {
            return _isEnabled;
        }

        public bool IsEnabled(T action)
        {
            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to check IsEnabled. Action for key '{action}' is not registered.");
                return false;
            }

            return inputAction.enabled;
        }

        public TValue ReadValue<TValue>(T action) where TValue : struct
        {
            if (_isEnabled == false)
            {
                return default;
            }
            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to ReadValue. Action for key '{action}' is not registered.");
                return default;
            }
            return inputAction.ReadValue<TValue>();
        }

        public bool IsPressed(T action)
        {
            if (_isEnabled == false)
            {
                return false;
            }
            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to check IsPressed. Action for key '{action}' is not registered.");
                return false;
            }
            return inputAction.IsPressed();
        }

        public bool WasPressedThisFrame(T action)
        {
            if (_isEnabled == false)
            {
                return false;
            }
            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to check WasPressedThisFrame. Action for key '{action}' is not registered.");
                return false;
            }
            return inputAction.WasPressedThisFrame();
        }

        public bool WasReleasedThisFrame(T action)
        {
            if (_isEnabled == false)
            {
                return false;
            }
            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to check WasReleasedThisFrame. Action for key '{action}' is not registered.");
                return false;
            }
            return inputAction.WasReleasedThisFrame();
        }

        public IInputSubscription SubscribeStarted(T action, Action<InputAction.CallbackContext> callback)
        {
            if (callback == null)
            {
                return null;
            }

            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to SubscribeStarted. Action for key '{action}' is not registered.");
                return null;
            }

            if (_startedCallbacks.ContainsKey((action, callback)))
            {
                Debug.LogWarning($"Callback for action '{action}' started event is already subscribed. Ignoring duplicate subscription.");
                return null;
            }

            Action<InputAction.CallbackContext> wrappedCallback = (context) =>
            {
                if (_isEnabled)
                {
                    callback(context);
                }
            };

            _startedCallbacks[(action, callback)] = wrappedCallback;
            inputAction.started += wrappedCallback;

            return new InputSubscription(() => UnsubscribeStarted(action, callback));
        }

        public IInputSubscription SubscribePerformed(T action, Action<InputAction.CallbackContext> callback)
        {
            if (callback == null)
            {
                return null;
            }

            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to SubscribePerformed. Action for key '{action}' is not registered.");
                return null;
            }

            if (_performedCallbacks.ContainsKey((action, callback)))
            {
                Debug.LogWarning($"Callback for action '{action}' performed event is already subscribed. Ignoring duplicate subscription.");
                return null;
            }

            Action<InputAction.CallbackContext> wrappedCallback = (context) =>
            {
                if (_isEnabled)
                {
                    callback(context);
                }
            };

            _performedCallbacks[(action, callback)] = wrappedCallback;
            inputAction.performed += wrappedCallback;

            return new InputSubscription(() => UnsubscribePerformed(action, callback));
        }

        public IInputSubscription SubscribeCanceled(T action, Action<InputAction.CallbackContext> callback)
        {
            if (callback == null)
            {
                return null;
            }

            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to SubscribeCanceled. Action for key '{action}' is not registered.");
                return null;
            }

            if (_canceledCallbacks.ContainsKey((action, callback)))
            {
                Debug.LogWarning($"Callback for action '{action}' canceled event is already subscribed. Ignoring duplicate subscription.");
                return null;
            }

            Action<InputAction.CallbackContext> wrappedCallback = (context) =>
            {
                if (_isEnabled)
                {
                    callback(context);
                }
            };

            _canceledCallbacks[(action, callback)] = wrappedCallback;
            inputAction.canceled += wrappedCallback;

            return new InputSubscription(() => UnsubscribeCanceled(action, callback));
        }

        public void UnsubscribeStarted(T action, Action<InputAction.CallbackContext> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to UnsubscribeStarted. Action for key '{action}' is not registered.");
                return;
            }

            if (!_startedCallbacks.TryGetValue((action, callback), out var wrappedCallback))
            {
                Debug.LogError($"Failed to UnsubscribeStarted. Callback for action '{action}' started event is not subscribed.");
                return;
            }

            inputAction.started -= wrappedCallback;
            _startedCallbacks.Remove((action, callback));
        }

        public void UnsubscribePerformed(T action, Action<InputAction.CallbackContext> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to UnsubscribePerformed. Action for key '{action}' is not registered.");
                return;
            }

            if (!_performedCallbacks.TryGetValue((action, callback), out var wrappedCallback))
            {
                Debug.LogError($"Failed to UnsubscribePerformed. Callback for action '{action}' performed event is not subscribed.");
                return;
            }

            inputAction.performed -= wrappedCallback;
            _performedCallbacks.Remove((action, callback));
        }

        public void UnsubscribeCanceled(T action, Action<InputAction.CallbackContext> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (!_actions.TryGetValue(action, out var inputAction))
            {
                Debug.LogError($"Failed to UnsubscribeCanceled. Action for key '{action}' is not registered.");
                return;
            }

            if (!_canceledCallbacks.TryGetValue((action, callback), out var wrappedCallback))
            {
                Debug.LogError($"Failed to UnsubscribeCanceled. Callback for action '{action}' canceled event is not subscribed.");
                return;
            }

            inputAction.canceled -= wrappedCallback;
            _canceledCallbacks.Remove((action, callback));
        }

        public void Dispose()
        {
            _inputActionAsset = null;
            _actions?.Clear();
            _actions = null;
            _startedCallbacks?.Clear();
            _startedCallbacks = null;
            _performedCallbacks?.Clear();
            _performedCallbacks = null;
            _canceledCallbacks?.Clear();
            _canceledCallbacks = null;
            _isEnabled = false;
        }
    }
}

