using System.Collections.Generic;

namespace Unleash.Internal
{
    /// <inheritdoc />
    /// <summary>
    /// Provides synchronization control that supports multiple readers and single writer over a ToggleCollection.
    /// </summary>
    internal sealed class ThreadSafeToggleCollection : ReaderWriterLockSlimOf<ToggleCollection>
    {
    }

    public class ToggleCollection
    {
        public int Version = 1;

        private readonly Dictionary<string, FeatureToggle> _togglesCache;

        private readonly Dictionary<int, Segment> _segmentsCache;

        public ToggleCollection(ICollection<FeatureToggle>? features = null, ICollection<Segment>? segments = null)
        {
            Features = features ?? new List<FeatureToggle>(0);
            Segments = segments ?? new List<Segment>(0);

            _togglesCache = new Dictionary<string, FeatureToggle>(Features.Count);
            _segmentsCache = new Dictionary<int, Segment>(Segments.Count);

            foreach (var featureToggle in Features)
            {
                _togglesCache.Add(featureToggle.Name, featureToggle);
            }

            foreach (var segment in Segments)
            {
                _segmentsCache.Add(segment.Id, segment);
            }
        }

        public ICollection<FeatureToggle> Features { get; }

        public ICollection<Segment> Segments { get; }

        public FeatureToggle GetToggleByName(string name)
        {
            return _togglesCache.TryGetValue(name, out var value)
                ? value
                : null;
        }

        public Segment GetSegmentById(int id)
        {
            return _segmentsCache.TryGetValue(id, out var value)
                ? value
                : null;
        }
    }
}