using System;
using System.Collections.Generic;
using PipetteGames.TypeSafeInputSystem.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PipetteGames.Inputs.Implements
{
    public class TypedInputSystem<T> : ITypedInputSystem<T> where T : Enum
    {
        private InputActionAsset _inputActionAsset;
        private Dictionary<T, InputAction> _actions;

        private bool _isEnabled;

        public TypedInputSystem(InputActionAsset inputActionAsset)
        {
            if (inputActionAsset == null)
            {
                throw new ArgumentNullException(nameof(inputActionAsset));
            }
            _inputActionAsset = inputActionAsset;
            _actions = new();
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
            if (_actions.TryGetValue(action, out var inputAction))
            {
                inputAction.Enable();
            }
            else
            {
                Debug.LogError($"Failed to Enable action. Action for key '{action}' is not registered.");
            }
        }

        public void Disable(T action)
        {
            if (_actions.TryGetValue(action, out var inputAction))
            {
                inputAction.Disable();
            }
            else
            {
                Debug.LogError($"Failed to Disable action. Action for key '{action}' is not registered.");
            }
        }

        public bool IsEnabled()
        {
            return _isEnabled;
        }

        public bool IsEnabled(T action)
        {
            if (_actions.TryGetValue(action, out var inputAction))
            {
                return inputAction.enabled;
            }
            else
            {
                Debug.LogError($"Failed to check IsEnabled. Action for key '{action}' is not registered.");
                return false;
            }
        }

        public TValue ReadValue<TValue>(T action) where TValue : struct
        {
            if (_isEnabled == false)
            {
                return default;
            }
            if (_actions.TryGetValue(action, out var inputAction))
            {
                return inputAction.ReadValue<TValue>();
            }
            else
            {
                Debug.LogError($"Failed to ReadValue. Action for key '{action}' is not registered.");
                return default;
            }
        }

        public bool IsPressed(T action)
        {
            if (_isEnabled == false)
            {
                return false;
            }
            if (_actions.TryGetValue(action, out var inputAction))
            {
                return inputAction.IsPressed();
            }
            else
            {
                Debug.LogError($"Failed to check IsPressed. Action for key '{action}' is not registered.");
                return false;
            }
        }

        public bool WasPressedThisFrame(T action)
        {
            if (_isEnabled == false)
            {
                return false;
            }
            if (_actions.TryGetValue(action, out var inputAction))
            {
                return inputAction.WasPressedThisFrame();
            }
            else
            {
                Debug.LogError($"Failed to check WasPressedThisFrame. Action for key '{action}' is not registered.");
                return false;
            }
        }

        public bool WasReleasedThisFrame(T action)
        {
            if (_isEnabled == false)
            {
                return false;
            }
            if (_actions.TryGetValue(action, out var inputAction))
            {
                return inputAction.WasReleasedThisFrame();
            }
            else
            {
                Debug.LogError($"Failed to check WasReleasedThisFrame. Action for key '{action}' is not registered.");
                return false;
            }
        }

        public void Dispose()
        {
            _inputActionAsset = null;
            _actions?.Clear();
            _actions = null;
            _isEnabled = false;
        }
    }
}
