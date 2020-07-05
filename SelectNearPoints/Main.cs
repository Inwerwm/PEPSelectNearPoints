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

                // 近傍判定半径の入力を求める
                bool inputRadiusIsValid = false;
                float radius = 0;
                while (!inputRadiusIsValid)
                {
                    var inputRadius = Microsoft.VisualBasic.Interaction.InputBox($"頂点ごとの検索半径を指定してください。", "選択頂点の近隣頂点を選択", "0.01", -1, -1);
                    if (inputRadius == "")
                        return;
                    inputRadiusIsValid = float.TryParse(inputRadius, out radius);
                    if (!inputRadiusIsValid)
                    {
                        MessageBox.Show("値が不正です。再入力してください。");
                    }
                    if (radius < 0)
                    {
                        MessageBox.Show("半径は正数にしてください。");
                        inputRadiusIsValid = false;
                    }
                }

                // 選択頂点を取得
                var selectedVertices = args.Host.Connector.View.PmxView.GetSelectedVertexIndices().Select(i => pmx.Vertex[i]);
                // 選択材質を取得
                int selectedMaterialIndex = args.Host.Connector.Form.SelectedMaterialIndex;

                // 選択頂点の範囲を取得(半径考慮)
                (float min, float max) XRange = (selectedVertices.First().Position.X, selectedVertices.First().Position.X);
                (float min, float max) YRange = (selectedVertices.First().Position.Y, selectedVertices.First().Position.Y);
                (float min, float max) ZRange = (selectedVertices.First().Position.Z, selectedVertices.First().Position.Z);
                foreach (var v in selectedVertices)
                {
                    if (XRange.min > v.Position.X)
                        XRange.min = v.Position.X;
                    if (YRange.min > v.Position.Y)
                        YRange.min = v.Position.Y;
                    if (ZRange.min > v.Position.Z)
                        ZRange.min = v.Position.Z;

                    if (XRange.max < v.Position.X)
                        XRange.max = v.Position.X;
                    if (YRange.max < v.Position.Y)
                        YRange.max = v.Position.Y;
                    if (ZRange.max < v.Position.Z)
                        ZRange.max = v.Position.Z;
                }
                XRange.min -= radius;
                YRange.min -= radius;
                ZRange.min -= radius;

                XRange.max += radius;
                YRange.max += radius;
                ZRange.max += radius;

                // 選択材質内の頂点からKd木を作成
                var targetVertices = pmx.Vertex
                     .Where(v => v.Position.X.IsWithin(XRange.min, XRange.max))
                     .Where(v => v.Position.Y.IsWithin(YRange.min, YRange.max))
                     .Where(v => v.Position.Z.IsWithin(ZRange.min, ZRange.max))
                     .Where(v => !selectedVertices.Contains(v));


                var tree = new KdTree<float, int>(3, new FloatMath(), AddDuplicateBehavior.List);
                foreach (var v in targetVertices)
                {
                    tree.Add(v.Position.ToArray(), pmx.Vertex.IndexOf(v));
                }
                // 選択頂点をクエリとして新しい選択頂点リストを作成
                var selectIDs = new List<int>();
                foreach (var v in selectedVertices)
                {
                    KdTreeNode<float, int>[] node = tree.RadialSearch(v.Position.ToArray(), radius);
                    selectIDs.AddRange(node.SelectMany(n => n.Duplicate));
                }

                args.Host.Connector.View.PmxView.SetSelectedVertexIndices(selectIDs.ToArray());
                MessageBox.Show(selectIDs.Count.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}