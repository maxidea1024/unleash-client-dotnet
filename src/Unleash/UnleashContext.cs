using System;
using System.Collections.Generic;

namespace Unleash
{
    /// <summary>
    /// A context which the feature request should be validated against. Usually scoped to a web request through an implementation of IUnleashContextProvider.
    /// </summary>
    public class UnleashContext : Yggdrasil.Context
    {
        public UnleashContext()
        {
            Properties = new Dictionary<string, string>();
        }

        public UnleashContext(string appName, string environment, string userId, string sessionId, string remoteAddress, DateTimeOffset? currentTime, Dictionary<string, string> properties)
        {
            AppName = appName;
            Environment = environment;
            UserId = userId;
            SessionId = sessionId;
            RemoteAddress = remoteAddress;
            CurrentTime = currentTime;
            Properties = properties;
        }

        public string GetByName(string contextName)
        {
            switch (contextName)
            {
                case "environment":
                    return Environment;
                case "appName":
                    return AppName;
                case "userId":
                    return UserId;
                case "sessionId":
                    return SessionId;
                case "remoteAddress":
                    return RemoteAddress;
                case "currentTime":
                    return (CurrentTime ?? DateTimeOffset.UtcNow).ToString("O");
                default:
                    string result;
                    Properties.TryGetValue(contextName, out result);
                    return result;
            }
        }

        public UnleashContext ApplyStaticFields(UnleashSettings settings)
        {
            Environment = string.IsNullOrEmpty(Environment) ? settings.Environment : Environment;
            AppName = string.IsNullOrEmpty(AppName) ? settings.AppName : AppName;
            return this;
        }

        internal static Builder New()
        {
            return new Builder();
        }

        internal class Builder
        {
            private string _appName;
            private string _environment;
            private string _userId;
            private string _sessionId;
            private string _remoteAddress;
            private DateTimeOffset? _currentTime;
            private readonly Dictionary<string, string> _properties;

            public Builder()
            {
                _properties = new Dictionary<string, string>();
            }

            public Builder(UnleashContext context)
            {
                _appName = context.AppName;
                _environment = context.Environment;
                _userId = context.UserId;
                _sessionId = context.SessionId;
                _remoteAddress = context.RemoteAddress;
                _currentTime = context.CurrentTime;
                _properties = new Dictionary<string, string>(context.Properties);
            }

            public Builder AppName(string appName)
            {
                _appName = appName;
                return this;
            }

            public Builder Environment(string environment)
            {
                _environment = environment;
                return this;
            }

            public Builder UserId(string userId)
            {
                _userId = userId;
                return this;
            }

            public Builder SessionId(string sessionId)
            {
                _sessionId = sessionId;
                return this;
            }

            public Builder RemoteAddress(string remoteAddress)
            {
                _remoteAddress = remoteAddress;
                return this;
            }

            public Builder CurrentTime(DateTimeOffset currentTime)
            {
                _currentTime = currentTime;
                return this;
            }

            public Builder Now()
            {
                _currentTime = DateTimeOffset.UtcNow;
                return this;
            }

            public Builder AddProperty(string name, string value)
            {
                _properties.Add(name, value);
                return this;
            }

            public UnleashContext Build()
                => new UnleashContext(_appName, _environment, _userId, _sessionId, _remoteAddress, _currentTime, _properties);
        }
    }
}
