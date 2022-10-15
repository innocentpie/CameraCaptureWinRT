using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

namespace CameraCaptureWinRT
{
    public class SourceDescription
    {
        /// <summary>
        /// Creates SourceDescription from the group and source pair.
        /// <br/>
        /// If only the ids are known, use <see cref="FromIdsAsync(string, string)"/>
        /// </summary>
        /// <param name="group"></param>
        /// <param name="source"></param>
        public SourceDescription(MediaFrameSourceGroup group, MediaFrameSourceInfo source)
        {
            SourceGroup = group;
            SourceInfo = source;
            
            SupportedResolutions = source.VideoProfileMediaDescription.Select(x => new ResolutionDescription(x)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Finds the source by the ids and creates a <see cref="SourceDescription"/>
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public async Task<SourceDescription> FromIdsAsync(string groupId, string sourceId)
        {
            var group = await MediaFrameSourceGroup.FromIdAsync(groupId);
            var source = group?.SourceInfos.First(x => x.Id == sourceId);

            if (source == null)
                return null;

            return new SourceDescription(group, source);
        }

        /// <summary>
        /// The SourceGroup's id
        /// </summary>
        public string GroupId => SourceGroup.Id;
        /// <summary>
        /// The SourceInfo's id
        /// </summary>
        public string SourceId => SourceInfo.Id;

        /// <summary>
        /// The underlying SourceGroup object
        /// </summary>
        public MediaFrameSourceGroup SourceGroup { get; }
        /// <summary>
        /// The underlying SourceInfo object
        /// </summary>
        public MediaFrameSourceInfo SourceInfo { get; }
        /// <summary>
        /// Returns the list of supported resolutions by the source
        /// </summary>
        public IReadOnlyList<ResolutionDescription> SupportedResolutions { get; }


        /// <summary>
        /// Checks if two <see cref="SourceDescription"/>s refer to the same source paired with the same group
        /// </summary>
        /// <param name="sourceDescription"></param>
        /// <returns></returns>
        public bool SourceAndGroupEquals(SourceDescription sourceDescription)
        {
            return this.SourceId == sourceDescription.SourceId
                && this.GroupId == sourceDescription.GroupId;
        }
    }
}
