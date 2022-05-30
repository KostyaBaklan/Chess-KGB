namespace Engine.Models.Config
{
    public class KingSafetyEvaluation
    {
        public double AttackValueFactor { get; set; }
        public int[] PieceAttackValue { get; set; }
        public double[] AttackWeight { get; set; }
    }
}