using System;
using UnityEngine.InputSystem;

namespace PipetteGames.TypeSafeInputSystem.Interfaces
{
    /// <summary>
    /// TypedInputSystem のインターフェース
    /// </summary>
    /// <typeparam name="T">アクションを識別する列挙型</typeparam>
    public interface ITypedInputSystem<T> : IDisposable where T : Enum
    {
        /// <summary>
        /// アクションを登録する
        /// </summary>
        /// <param name="actionMapName">アクションマップ名</param>
        /// <param name="key">アクション識別キー</param>
        /// <remarks>アクション名はアクション識別キーの名前が使用されます</remarks>
        void RegisterAction(string actionMapName, T key);
        /// <summary>
        /// アクションを登録する
        /// </summary>
        /// <param name="actionMapName">アクションマップ名</param>
        /// <param name="key">アクション識別キー</param>
        /// <param name="actionName">アクション名</param>
        void RegisterAction(string actionMapName, T key, string actionName);
        /// <summary>
        /// InputSystem を有効化する
        /// </summary>
        void Enable();
        /// <summary>
        /// InputSystem を無効化する
        /// InputSystem が無効な間、すべてのアクションは無効化されます
        /// </summary>
        void Disable();
        /// <summary>
        /// 特定のアクションを有効化する
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        void Enable(T action);
        /// <summary>
        /// 特定のアクションを無効化する
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        void Disable(T action);
        /// <summary>
        /// InputSystem が有効か
        /// </summary>
        bool IsEnabled();
        /// <summary>
        /// 特定のアクションが有効か
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        bool IsEnabled(T action);
        /// <summary>
        /// 特定のアクションが押されているか
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        bool IsPressed(T action);
        /// <summary>
        /// 特定のアクションがこのフレームで押されたか
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        bool WasPressedThisFrame(T action);
        /// <summary>
        /// 特定のアクションがこのフレームで離されたか
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        bool WasReleasedThisFrame(T action);
        /// <summary>
        /// 特定のアクションの値を取得する
        /// </summary>
        /// <typeparam name="TValue">値の型</typeparam>
        /// <param name="action">アクション識別キー</param>
        TValue ReadValue<TValue>(T action) where TValue : struct;
        /// <summary>
        /// アクションが開始されたときのコールバックを登録
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        /// <param name="callback">実行するコールバック</param>
        /// <returns>購読を管理する IInputSubscription。Dispose() で購読解除可能</returns>
        IInputSubscription RegisterStarted(T action, Action<InputAction.CallbackContext> callback);
        /// <summary>
        /// アクションが実行されたときのコールバックを登録
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        /// <param name="callback">実行するコールバック</param>
        /// <returns>購読を管理する IInputSubscription。Dispose() で購読解除可能</returns>
        IInputSubscription RegisterPerformed(T action, Action<InputAction.CallbackContext> callback);
        /// <summary>
        /// アクションがキャンセルされたときのコールバックを登録
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        /// <param name="callback">実行するコールバック</param>
        /// <returns>購読を管理する IInputSubscription。Dispose() で購読解除可能</returns>
        IInputSubscription RegisterCanceled(T action, Action<InputAction.CallbackContext> callback);
        /// <summary>
        /// アクションが開始されたときのコールバックを登録解除
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        /// <param name="callback">登録解除するコールバック</param>
        void UnregisterStarted(T action, Action<InputAction.CallbackContext> callback);
        /// <summary>
        /// アクションが実行されたときのコールバックを登録解除
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        /// <param name="callback">登録解除するコールバック</param>
        void UnregisterPerformed(T action, Action<InputAction.CallbackContext> callback);
        /// <summary>
        /// アクションがキャンセルされたときのコールバックを登録解除
        /// </summary>
        /// <param name="action">アクション識別キー</param>
        /// <param name="callback">登録解除するコールバック</param>
        void UnregisterCanceled(T action, Action<InputAction.CallbackContext> callback);
    }
}
