﻿using ExpectedObjects;

namespace SlackConnector.Tests.Unit.TestExtensions
{
    public static class ExpectedObjectExtensions
    {
        public static void ShouldLookLike<T>(this T actual, T expected)
        {
            expected.ToExpectedObject().ShouldEqual(actual);
        }
    }
}