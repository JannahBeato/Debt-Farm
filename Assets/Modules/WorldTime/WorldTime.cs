using System;
using System.Collections;
using UnityEngine;

namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public event EventHandler<TimeSpan> WorldTimeChanged;

        [SerializeField] private float _dayLength;

        private TimeSpan _currentTime = TimeSpan.FromHours(7);

        private float _minuteLength => _dayLength / WorldTimeConstant.MinutesInDay;

        private void Start()
        {
            StartCoroutine(AddMinute());
        }

        private IEnumerator AddMinute()
        {
            while (true)
            {
                _currentTime += TimeSpan.FromMinutes(1);

                if (_currentTime.TotalMinutes >= WorldTimeConstant.MinutesInDay)
                    _currentTime -= TimeSpan.FromMinutes(WorldTimeConstant.MinutesInDay);

                WorldTimeChanged?.Invoke(this, _currentTime);

                yield return new WaitForSeconds(_minuteLength);
            }
        }

        public int GetMinutesOfDay()
        {
            return (int)_currentTime.TotalMinutes;
        }

        public void SetMinutesOfDay(int minutes)
        {
            minutes = Mathf.Clamp(minutes, 0, WorldTimeConstant.MinutesInDay - 1);
            _currentTime = TimeSpan.FromMinutes(minutes);

            WorldTimeChanged?.Invoke(this, _currentTime);
        }
    }
}
