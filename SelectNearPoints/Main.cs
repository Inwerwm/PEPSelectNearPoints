using KdTree;
using KdTree.Math;
using PEPlugin;
using PEPlugin.Pmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SelectNearPoints
{
    public class SelectNearPoints : PEPluginClass
    {
        public SelectNearPoints() : base()
        {
        }

        public override string Name
        {
            get
            {
                return "選択頂点の近隣頂点を選択";
            }
        }

        public override string Version
        {
            get
            {
                return "1.0";
            }
        }

        public override string Description
        {
            get
            {
                return "選択頂点の近隣頂点を選択";
            }
        }

        public override IPEPluginOption Option
        {
            get
            {
                // boot時実行, プラグインメニューへの登録, メニュー登録名
                return new PEPluginOption(false, true, "選択頂点の近隣頂点を選択");
            }
        }

        public override void Run(IPERunArgs args)
        {
            try
            {
                var pmx = args.Host.Connector.Pmx.GetCurrentState();
                var radius = float.Parse(Microsoft.VisualBasic.Interaction.InputBox("頂点ごとの検索半径を指定してください", "選択頂点の近隣頂点を選択", "0.01", -1, -1));

                // 選択頂点を取得
                var selectedVertexIndices = args.Host.Connector.View.PmxView.GetSelectedVertexIndices();
                // 選択材質を取得
                int selectedMaterialIndex = args.Host.Connector.Form.SelectedMaterialIndex;

                // 選択材質内の頂点からKd木を作成(選択材質がなければ全頂点を対象)
                List<IPXVertex> targetVertices;
                if (selectedMaterialIndex >= 0)
                {
                    var selectedMaterial = pmx.Material[selectedMaterialIndex];
                    targetVertices = Utility.GetMaterialVertices(selectedMaterial);
                }
                else
                {
                    targetVertices = pmx.Vertex.ToList();
                }

                var tree = new KdTree<float, int>(3, new FloatMath());
                foreach (var v in targetVertices)
                {
                    tree.Add(v.Position.ToArray(), pmx.Vertex.IndexOf(v));
                }
                // 選択頂点をクエリとして新しい選択頂点リストを作成
                var selectIDs = new List<int>();
                foreach (var v in selectedVertexIndices.Select(i => pmx.Vertex[i]))
                {
                    KdTreeNode<float, int>[] node = tree.RadialSearch(v.Position.ToArray(), radius);
                    selectIDs.AddRange(node.Select(n => n.Value));
                }
                selectIDs.Distinct();

                args.Host.Connector.View.PmxView.SetSelectedVertexIndices(selectIDs.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}