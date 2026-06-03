namespace SistemaTraction.Domain.Dtf;

public enum DtfMovementType
{
    Entrada = 1,  // estampas recebidas (entrada de folhas convertida)
    Saida = 2,    // estampas consumidas em produção
    Ajuste = 3    // correção manual de inventário (em estampas)
}
