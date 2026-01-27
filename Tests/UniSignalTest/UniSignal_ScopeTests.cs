using NUnit.Framework;
using UniCore.Signal;

namespace UniCore.Tests.Signal
{
    public class UniSignal_ScopeTests
    {
        public static readonly SignalScope Gameplay = new(1UL << 0);
        public static readonly SignalScope UI = new(1UL << 1);

        [Test]
        public void UniSignal_MatchScope_Test()
        {
            var listenerAllScope = SignalScope.All;
            var listenerGameplayScope = Gameplay;
            var listenerUIScope = UI;

            var sendAll = SignalScope.All;
            Assert.IsTrue(listenerAllScope.Intersects(sendAll));
            Assert.IsTrue(listenerGameplayScope.Intersects(sendAll));
            Assert.IsTrue(listenerUIScope.Intersects(sendAll));

            var sendGameplay = Gameplay;
            Assert.IsTrue(listenerAllScope.Intersects(sendGameplay));
            Assert.IsTrue(listenerGameplayScope.Intersects(sendGameplay));
            Assert.IsFalse(listenerUIScope.Intersects(sendGameplay));

            var sendUI = UI;
            Assert.IsTrue(listenerAllScope.Intersects(sendUI));
            Assert.IsFalse(listenerGameplayScope.Intersects(sendUI));
            Assert.IsTrue(listenerUIScope.Intersects(sendUI));
        }
    }
}