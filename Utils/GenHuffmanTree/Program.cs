﻿
uint[] codes = [
    0b_11111111_11000000_00000000_00000000,
    0b_11111111_11111111_10110000_00000000,
    0b_11111111_11111111_11111110_00100000,
    0b_11111111_11111111_11111110_00110000,
    0b_11111111_11111111_11111110_01000000,
    0b_11111111_11111111_11111110_01010000,
    0b_11111111_11111111_11111110_01100000,
    0b_11111111_11111111_11111110_01110000,
    0b_11111111_11111111_11111110_10000000,
    0b_11111111_11111111_11101010_00000000,
    0b_11111111_11111111_11111111_11110000,
    0b_11111111_11111111_11111110_10010000,
    0b_11111111_11111111_11111110_10100000,
    0b_11111111_11111111_11111111_11110100,
    0b_11111111_11111111_11111110_10110000,
    0b_11111111_11111111_11111110_11000000,
    0b_11111111_11111111_11111110_11010000,
    0b_11111111_11111111_11111110_11100000,
    0b_11111111_11111111_11111110_11110000,
    0b_11111111_11111111_11111111_00000000,
    0b_11111111_11111111_11111111_00010000,
    0b_11111111_11111111_11111111_00100000,
    0b_11111111_11111111_11111111_11111000,
    0b_11111111_11111111_11111111_00110000,
    0b_11111111_11111111_11111111_01000000,
    0b_11111111_11111111_11111111_01010000,
    0b_11111111_11111111_11111111_01100000,
    0b_11111111_11111111_11111111_01110000,
    0b_11111111_11111111_11111111_10000000,
    0b_11111111_11111111_11111111_10010000,
    0b_11111111_11111111_11111111_10100000,
    0b_11111111_11111111_11111111_10110000,
    0b_01010000_00000000_00000000_00000000,
    0b_11111110_00000000_00000000_00000000,
    0b_11111110_01000000_00000000_00000000,
    0b_11111111_10100000_00000000_00000000,
    0b_11111111_11001000_00000000_00000000,
    0b_01010100_00000000_00000000_00000000,
    0b_11111000_00000000_00000000_00000000,
    0b_11111111_01000000_00000000_00000000,
    0b_11111110_10000000_00000000_00000000,
    0b_11111110_11000000_00000000_00000000,
    0b_11111001_00000000_00000000_00000000,
    0b_11111111_01100000_00000000_00000000,
    0b_11111010_00000000_00000000_00000000,
    0b_01011000_00000000_00000000_00000000,
    0b_01011100_00000000_00000000_00000000,
    0b_01100000_00000000_00000000_00000000,
    0b_00000000_00000000_00000000_00000000,
    0b_00001000_00000000_00000000_00000000,
    0b_00010000_00000000_00000000_00000000,
    0b_01100100_00000000_00000000_00000000,
    0b_01101000_00000000_00000000_00000000,
    0b_01101100_00000000_00000000_00000000,
    0b_01110000_00000000_00000000_00000000,
    0b_01110100_00000000_00000000_00000000,
    0b_01111000_00000000_00000000_00000000,
    0b_01111100_00000000_00000000_00000000,
    0b_10111000_00000000_00000000_00000000,
    0b_11111011_00000000_00000000_00000000,
    0b_11111111_11111000_00000000_00000000,
    0b_10000000_00000000_00000000_00000000,
    0b_11111111_10110000_00000000_00000000,
    0b_11111111_00000000_00000000_00000000,
    0b_11111111_11010000_00000000_00000000,
    0b_10000100_00000000_00000000_00000000,
    0b_10111010_00000000_00000000_00000000,
    0b_10111100_00000000_00000000_00000000,
    0b_10111110_00000000_00000000_00000000,
    0b_11000000_00000000_00000000_00000000,
    0b_11000010_00000000_00000000_00000000,
    0b_11000100_00000000_00000000_00000000,
    0b_11000110_00000000_00000000_00000000,
    0b_11001000_00000000_00000000_00000000,
    0b_11001010_00000000_00000000_00000000,
    0b_11001100_00000000_00000000_00000000,
    0b_11001110_00000000_00000000_00000000,
    0b_11010000_00000000_00000000_00000000,
    0b_11010010_00000000_00000000_00000000,
    0b_11010100_00000000_00000000_00000000,
    0b_11010110_00000000_00000000_00000000,
    0b_11011000_00000000_00000000_00000000,
    0b_11011010_00000000_00000000_00000000,
    0b_11011100_00000000_00000000_00000000,
    0b_11011110_00000000_00000000_00000000,
    0b_11100000_00000000_00000000_00000000,
    0b_11100010_00000000_00000000_00000000,
    0b_11100100_00000000_00000000_00000000,
    0b_11111100_00000000_00000000_00000000,
    0b_11100110_00000000_00000000_00000000,
    0b_11111101_00000000_00000000_00000000,
    0b_11111111_11011000_00000000_00000000,
    0b_11111111_11111110_00000000_00000000,
    0b_11111111_11100000_00000000_00000000,
    0b_11111111_11110000_00000000_00000000,
    0b_10001000_00000000_00000000_00000000,
    0b_11111111_11111010_00000000_00000000,
    0b_00011000_00000000_00000000_00000000,
    0b_10001100_00000000_00000000_00000000,
    0b_00100000_00000000_00000000_00000000,
    0b_10010000_00000000_00000000_00000000,
    0b_00101000_00000000_00000000_00000000,
    0b_10010100_00000000_00000000_00000000,
    0b_10011000_00000000_00000000_00000000,
    0b_10011100_00000000_00000000_00000000,
    0b_00110000_00000000_00000000_00000000,
    0b_11101000_00000000_00000000_00000000,
    0b_11101010_00000000_00000000_00000000,
    0b_10100000_00000000_00000000_00000000,
    0b_10100100_00000000_00000000_00000000,
    0b_10101000_00000000_00000000_00000000,
    0b_00111000_00000000_00000000_00000000,
    0b_10101100_00000000_00000000_00000000,
    0b_11101100_00000000_00000000_00000000,
    0b_10110000_00000000_00000000_00000000,
    0b_01000000_00000000_00000000_00000000,
    0b_01001000_00000000_00000000_00000000,
    0b_10110100_00000000_00000000_00000000,
    0b_11101110_00000000_00000000_00000000,
    0b_11110000_00000000_00000000_00000000,
    0b_11110010_00000000_00000000_00000000,
    0b_11110100_00000000_00000000_00000000,
    0b_11110110_00000000_00000000_00000000,
    0b_11111111_11111100_00000000_00000000,
    0b_11111111_10000000_00000000_00000000,
    0b_11111111_11110100_00000000_00000000,
    0b_11111111_11101000_00000000_00000000,
    0b_11111111_11111111_11111111_11000000,
    0b_11111111_11111110_01100000_00000000,
    0b_11111111_11111111_01001000_00000000,
    0b_11111111_11111110_01110000_00000000,
    0b_11111111_11111110_10000000_00000000,
    0b_11111111_11111111_01001100_00000000,
    0b_11111111_11111111_01010000_00000000,
    0b_11111111_11111111_01010100_00000000,
    0b_11111111_11111111_10110010_00000000,
    0b_11111111_11111111_01011000_00000000,
    0b_11111111_11111111_10110100_00000000,
    0b_11111111_11111111_10110110_00000000,
    0b_11111111_11111111_10111000_00000000,
    0b_11111111_11111111_10111010_00000000,
    0b_11111111_11111111_10111100_00000000,
    0b_11111111_11111111_11101011_00000000,
    0b_11111111_11111111_10111110_00000000,
    0b_11111111_11111111_11101100_00000000,
    0b_11111111_11111111_11101101_00000000,
    0b_11111111_11111111_01011100_00000000,
    0b_11111111_11111111_11000000_00000000,
    0b_11111111_11111111_11101110_00000000,
    0b_11111111_11111111_11000010_00000000,
    0b_11111111_11111111_11000100_00000000,
    0b_11111111_11111111_11000110_00000000,
    0b_11111111_11111111_11001000_00000000,
    0b_11111111_11111110_11100000_00000000,
    0b_11111111_11111111_01100000_00000000,
    0b_11111111_11111111_11001010_00000000,
    0b_11111111_11111111_01100100_00000000,
    0b_11111111_11111111_11001100_00000000,
    0b_11111111_11111111_11001110_00000000,
    0b_11111111_11111111_11101111_00000000,
    0b_11111111_11111111_01101000_00000000,
    0b_11111111_11111110_11101000_00000000,
    0b_11111111_11111110_10010000_00000000,
    0b_11111111_11111111_01101100_00000000,
    0b_11111111_11111111_01110000_00000000,
    0b_11111111_11111111_11010000_00000000,
    0b_11111111_11111111_11010010_00000000,
    0b_11111111_11111110_11110000_00000000,
    0b_11111111_11111111_11010100_00000000,
    0b_11111111_11111111_01110100_00000000,
    0b_11111111_11111111_01111000_00000000,
    0b_11111111_11111111_11110000_00000000,
    0b_11111111_11111110_11111000_00000000,
    0b_11111111_11111111_01111100_00000000,
    0b_11111111_11111111_11010110_00000000,
    0b_11111111_11111111_11011000_00000000,
    0b_11111111_11111111_00000000_00000000,
    0b_11111111_11111111_00001000_00000000,
    0b_11111111_11111111_10000000_00000000,
    0b_11111111_11111111_00010000_00000000,
    0b_11111111_11111111_11011010_00000000,
    0b_11111111_11111111_10000100_00000000,
    0b_11111111_11111111_11011100_00000000,
    0b_11111111_11111111_11011110_00000000,
    0b_11111111_11111110_10100000_00000000,
    0b_11111111_11111111_10001000_00000000,
    0b_11111111_11111111_10001100_00000000,
    0b_11111111_11111111_10010000_00000000,
    0b_11111111_11111111_11100000_00000000,
    0b_11111111_11111111_10010100_00000000,
    0b_11111111_11111111_10011000_00000000,
    0b_11111111_11111111_11100010_00000000,
    0b_11111111_11111111_11111000_00000000,
    0b_11111111_11111111_11111000_01000000,
    0b_11111111_11111110_10110000_00000000,
    0b_11111111_11111110_00100000_00000000,
    0b_11111111_11111111_10011100_00000000,
    0b_11111111_11111111_11100100_00000000,
    0b_11111111_11111111_10100000_00000000,
    0b_11111111_11111111_11110110_00000000,
    0b_11111111_11111111_11111000_10000000,
    0b_11111111_11111111_11111000_11000000,
    0b_11111111_11111111_11111001_00000000,
    0b_11111111_11111111_11111011_11000000,
    0b_11111111_11111111_11111011_11100000,
    0b_11111111_11111111_11111001_01000000,
    0b_11111111_11111111_11110001_00000000,
    0b_11111111_11111111_11110110_10000000,
    0b_11111111_11111110_01000000_00000000,
    0b_11111111_11111111_00011000_00000000,
    0b_11111111_11111111_11111001_10000000,
    0b_11111111_11111111_11111100_00000000,
    0b_11111111_11111111_11111100_00100000,
    0b_11111111_11111111_11111001_11000000,
    0b_11111111_11111111_11111100_01000000,
    0b_11111111_11111111_11110010_00000000,
    0b_11111111_11111111_00100000_00000000,
    0b_11111111_11111111_00101000_00000000,
    0b_11111111_11111111_11111010_00000000,
    0b_11111111_11111111_11111010_01000000,
    0b_11111111_11111111_11111111_11010000,
    0b_11111111_11111111_11111100_01100000,
    0b_11111111_11111111_11111100_10000000,
    0b_11111111_11111111_11111100_10100000,
    0b_11111111_11111110_11000000_00000000,
    0b_11111111_11111111_11110011_00000000,
    0b_11111111_11111110_11010000_00000000,
    0b_11111111_11111111_00110000_00000000,
    0b_11111111_11111111_10100100_00000000,
    0b_11111111_11111111_00111000_00000000,
    0b_11111111_11111111_01000000_00000000,
    0b_11111111_11111111_11100110_00000000,
    0b_11111111_11111111_10101000_00000000,
    0b_11111111_11111111_10101100_00000000,
    0b_11111111_11111111_11110111_00000000,
    0b_11111111_11111111_11110111_10000000,
    0b_11111111_11111111_11110100_00000000,
    0b_11111111_11111111_11110101_00000000,
    0b_11111111_11111111_11111010_10000000,
    0b_11111111_11111111_11101000_00000000,
    0b_11111111_11111111_11111010_11000000,
    0b_11111111_11111111_11111100_11000000,
    0b_11111111_11111111_11111011_00000000,
    0b_11111111_11111111_11111011_01000000,
    0b_11111111_11111111_11111100_11100000,
    0b_11111111_11111111_11111101_00000000,
    0b_11111111_11111111_11111101_00100000,
    0b_11111111_11111111_11111101_01000000,
    0b_11111111_11111111_11111101_01100000,
    0b_11111111_11111111_11111111_11100000,
    0b_11111111_11111111_11111101_10000000,
    0b_11111111_11111111_11111101_10100000,
    0b_11111111_11111111_11111101_11000000,
    0b_11111111_11111111_11111101_11100000,
    0b_11111111_11111111_11111110_00000000,
    0b_11111111_11111111_11111011_10000000,
    0b_11111111_11111111_11111111_11111100
    ];

byte[] bitLengths =
        [
            13,
            23,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            24,
            30,
            28,
            28,
            30,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            30,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            6,
            10,
            10,
            12,
            13,
            6,
            8,
            11,
            10,
            10,
            8,
            11,
            8,
            6,
            6,
            6,
            5,
            5,
            5,
            6,
            6,
            6,
            6,
            6,
            6,
            6,
            7,
            8,
            15,
            6,
            12,
            10,
            13,
            6,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            7,
            8,
            7,
            8,
            13,
            19,
            13,
            14,
            6,
            15,
            5,
            6,
            5,
            6,
            5,
            6,
            6,
            6,
            5,
            7,
            7,
            6,
            6,
            6,
            5,
            6,
            7,
            6,
            5,
            5,
            6,
            7,
            7,
            7,
            7,
            7,
            15,
            11,
            14,
            13,
            28,
            20,
            22,
            20,
            20,
            22,
            22,
            22,
            23,
            22,
            23,
            23,
            23,
            23,
            23,
            24,
            23,
            24,
            24,
            22,
            23,
            24,
            23,
            23,
            23,
            23,
            21,
            22,
            23,
            22,
            23,
            23,
            24,
            22,
            21,
            20,
            22,
            22,
            23,
            23,
            21,
            23,
            22,
            22,
            24,
            21,
            22,
            23,
            23,
            21,
            21,
            22,
            21,
            23,
            22,
            23,
            23,
            20,
            22,
            22,
            22,
            23,
            22,
            22,
            23,
            26,
            26,
            20,
            19,
            22,
            23,
            22,
            25,
            26,
            26,
            26,
            27,
            27,
            26,
            24,
            25,
            19,
            21,
            26,
            27,
            27,
            26,
            27,
            24,
            21,
            21,
            26,
            26,
            28,
            27,
            27,
            27,
            20,
            24,
            20,
            21,
            22,
            21,
            21,
            23,
            22,
            22,
            25,
            25,
            24,
            24,
            26,
            23,
            26,
            27,
            26,
            26,
            27,
            27,
            27,
            27,
            27,
            28,
            27,
            27,
            27,
            27,
            27,
            26,
            30
        ];

var tree = new TreeNode?[15 * 256]; // each node has 256 children, and we have max 30 bit length so we have 4^2 - 1 nodes
/* node format
 * LXXXXXXXYYYYYYYY where
 * L: 1 if a leaf node, YYYYYYYY contains symbol
 * XXXXXXX: bit length, or next index if it is a middle node
 * YYYYYYYY: symbol or 0 if it is a middle node
*/

for (ushort i = 0; i <= 256; i++)
{
    var code = codes[i];
    var bitLength = bitLengths[i];

    ushort symbol = i;
    int nodeTableIndex = 0;
    int allocatedNodeTableIndex = 0;

    while (bitLength > 0)
    {
        var nodeIndex = (ushort)(code >> 24 /* take the most left 8 bits */);

        if (bitLength <= 8)
        {
            tree[(nodeTableIndex << 8) | nodeIndex] = new TreeNode();
            bitLength = 0;
        }
        else
        {
            var nodeValue = tree[nodeTableIndex << 8 | nodeIndex];

            if (nodeValue == null) // empty node
            {
                allocatedNodeTableIndex++;
                tree[nodeTableIndex << 8 | nodeIndex] = new TreeNode(false, 0, (byte)allocatedNodeTableIndex, (byte)nodeIndex);
                nodeTableIndex = allocatedNodeTableIndex;
            }
            else
            {
                nodeTableIndex = nodeValue.Value.NodeTableIndex;
            }

            code <<= 8;
            bitLength -= 8;
        }
    }
}

PrintTree();

void PrintTree()
{
    foreach (var node in tree)
    {
        
    }
}

struct TreeNode
{
    public bool IsLeaf;
    public byte Symbol;
    public byte NodeTableIndex;
    public byte NodeChildIndex;

    public TreeNode(bool isLeaf, byte symbol, byte nodeIndex, byte nodeChildIndex)
    {
        IsLeaf = isLeaf;
        Symbol = symbol;
        NodeTableIndex = nodeIndex;
        NodeChildIndex = nodeChildIndex;
    }
}