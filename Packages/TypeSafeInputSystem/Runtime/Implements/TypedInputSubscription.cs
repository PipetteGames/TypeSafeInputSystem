using System;
using PipetteGames.TypeSafeInputSystem.Interfaces;

namespace PipetteGames.TypeSafeInputSystem.Implements
{
    /// <summary>
    /// 入力イベントの購読を表す実装クラス
    /// </summary>
    internal class TypedInputSubscription : ITypedInputSubscription
    {
        private Action _unsubscribe;
        private bool _isDisposed;

        public TypedInputSubscription(Action unsubscribeAction)
        {
            _unsubscribe = unsubscribeAction ?? throw new ArgumentNullException(nameof(unsubscribeAction));
            _isDisposed = false;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _unsubscribe?.Invoke();
            _unsubscribe = null;
            _isDisposed = true;
        }
    }
}
