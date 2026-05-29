namespace SistemaTraction.Domain.Dtf;

public enum DtfMovementType
{
    Entrada = 1,  // folhas recebidas (compra/reposição)
    Saida = 2,    // folhas consumidas em produção
    Ajuste = 3    // correção manual de inventário
}
