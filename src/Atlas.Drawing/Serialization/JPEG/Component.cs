using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal class Component
    {
        public byte Id { get; set; }
        public byte HFactor { get; set; }
        public byte VFactor { get; set; }
        public byte QuantizationId { get; set; }
        public HuffmanTable ACTable { get; set; }
        public HuffmanTable DCTable { get; set; }
        public int[] QuantizationTable { get; set; }
        public int PreviousDC { get; set; }
        public int MaxV { get; set; }
        public int MaxH { get; set; }
        
        // TODO(Dan): Need to store the decoded data with the component?

        public Component(byte id, byte hFactor, byte vFactor, byte quantizationId)
        {
            Id = id;
            HFactor = hFactor;
            VFactor = vFactor;
            QuantizationId = quantizationId;
        }
    }
}
