using NUnit.Framework;
using UniCore.Signal;
using UnityEngine;

namespace UniCore.Tests.Signal
{
    public class UniSignal_DispatchTests
    {
        private struct TestSignal : ISignalEvent
        {
        }

        private class AllListener : ISignalListener<TestSignal>
        {
            public bool Received;

            public void OnSignal(TestSignal signal)
            {
                Received = true;
            }
        }

        private class GamePlayListener : ISignalListener<TestSignal>
        {
            public bool Received;
            public SignalScope ListenScope => UniSignal_ScopeTests.Gameplay;

            public void OnSignal(TestSignal signal)
            {
                Received = true;
            }
        }

        private class UIListener : ISignalListener<TestSignal>
        {
            public bool Received;
            public SignalScope ListenScope => UniSignal_ScopeTests.UI;

            public void OnSignal(TestSignal signal)
            {
                Received = true;
            }
        }

        [Test]
        public void UniSignal_Dispatch_ShouldNotifyListener_Test()
        {
            var allListener = new AllListener();
            var gamePlayListener = new GamePlayListener();
            var uiListener = new UIListener();

            SignalBus.Register(allListener);
            SignalBus.Register(gamePlayListener);
            SignalBus.Register(uiListener);

            SignalBus.Dispatch(new TestSignal());

            Assert.IsTrue(allListener.Received);
            Assert.IsTrue(gamePlayListener.Received);
            Assert.IsTrue(uiListener.Received);

            SignalBus.Unregister(allListener);
            SignalBus.Unregister(gamePlayListener);
            SignalBus.Unregister(uiListener);
        }

        [Test]
        public void UniSignal_Dispatch_ShouldNotNotify_WhenScopeDoesNotMatch_Test()
        {
            var allListener = new AllListener();
            var gamePlayListener = new GamePlayListener();
            var uiListener = new UIListener();

            SignalBus.Register(allListener);
            SignalBus.Register(gamePlayListener);
            SignalBus.Register(uiListener);

            SignalBus.Dispatch(new TestSignal(), UniSignal_ScopeTests.Gameplay);

            Assert.IsTrue(allListener.Received);
            Assert.IsTrue(gamePlayListener.Received);
            Assert.IsFalse(uiListener.Received);

            SignalBus.Unregister(allListener);
            SignalBus.Unregister(gamePlayListener);
            SignalBus.Unregister(uiListener);
        }

        [Test]
        public void UniSignal_Dispatch_ShouldRespectPriorityOrder_Test()
        {
            var order = new System.Collections.Generic.List<int>();

            var low = new OrderedListener(0, order);
            var high = new OrderedListener(100, order);

            SignalBus.Register(low);
            SignalBus.Register(high);

            SignalBus.Dispatch(new TestSignal());

            Assert.AreEqual(100, order[0]);
            Assert.AreEqual(0, order[1]);

            SignalBus.Unregister(low);
            SignalBus.Unregister(high);
        }

        private class OrderedListener : ISignalListener<TestSignal>
        {
            private readonly System.Collections.Generic.List<int> _order;

            public OrderedListener(int priority, System.Collections.Generic.List<int> order)
            {
                Priority = priority;
                _order = order;
            }

            public int Priority { get; }

            public void OnSignal(TestSignal signal)
            {
                _order.Add(Priority);
            }
        }

        [Test]
        public void UniSignal_Dispatch_ShouldWorkWithMultipleScopes_Test()
        {
            var listener = new TestListener();
            SignalBus.Register(listener);

            SignalBus.Dispatch(new TestSignalMultiScope());

            Assert.IsTrue(listener.Received);

            SignalBus.Unregister(listener);
        }

        private class TestListener : ISignalListener<TestSignalMultiScope>
        {
            public bool Received;
            public int Priority => 0;
            public SignalScope ListenScope => UniSignal_ScopeTests.Gameplay;

            public void OnSignal(TestSignalMultiScope signal)
            {
                Received = true;
            }
        }

        private struct TestSignalMultiScope : ISignalEvent
        {
            public SignalScope Scope => UniSignal_ScopeTests.Gameplay | UniSignal_ScopeTests.UI;
        }

        [Test]
        public void UniSignal_Dispatch_ShouldNotifyAllListeners_Test()
        {
            var a = new TestListener();
            var b = new TestListener();

            SignalBus.Register(a);
            SignalBus.Register(b);

            SignalBus.Dispatch(new TestSignalMultiScope());

            Assert.IsTrue(a.Received);
            Assert.IsTrue(b.Received);

            SignalBus.Unregister(a);
            SignalBus.Unregister(b);
        }

        [Test]
        public void UniSignal_Dispatch_ShouldContinue_WhenListenerThrows_Test()
        {
            var good = new TestListener();
            var bad = new ExceptionListener();

            SignalBus.Register(bad);
            SignalBus.Register(good);
            Assert.DoesNotThrow(() =>
            {
                Debug.unityLogger.logEnabled = false;
                SignalBus.Dispatch(new TestSignalMultiScope());
                Debug.unityLogger.logEnabled = true;
            });
            Assert.IsTrue(good.Received);

            SignalBus.Unregister(bad);
            SignalBus.Unregister(good);
        }

        private class ExceptionListener : ISignalListener<TestSignalMultiScope>
        {
            public int Priority => 0;
            public SignalScope ListenScope => UniSignal_ScopeTests.Gameplay;

            public void OnSignal(TestSignalMultiScope signal)
            {
                throw new System.Exception("Boom");
            }
        }
    }
}