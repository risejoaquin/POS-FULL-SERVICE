import re

with open('./PosBuilder/SqlGenerator.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# I will add the insert for product modifiers at the end, right before return sb.ToString();
seed_modifiers = """
            sb.AppendLine($"INSERT INTO \\"ProductModifiers\\" (\\"Id\\", \\"Name\\", \\"IsRequired\\", \\"MinSelections\\", \\"MaxSelections\\", \\"TenantId\\") VALUES (1, 'Tipo de Leche', true, 1, 1, '{model.TenantId}') ON CONFLICT DO NOTHING;");
            sb.AppendLine($"INSERT INTO \\"ModifierOptions\\" (\\"ProductModifierId\\", \\"Name\\", \\"PriceAdjustment\\", \\"IsDefault\\", \\"TenantId\\") VALUES (1, 'Entera', 0, true, '{model.TenantId}'), (1, 'Deslactosada', 5, false, '{model.TenantId}'), (1, 'Almendra', 10, false, '{model.TenantId}') ON CONFLICT DO NOTHING;");
            sb.AppendLine($"INSERT INTO \\"ProductModifiers\\" (\\"Id\\", \\"Name\\", \\"IsRequired\\", \\"MinSelections\\", \\"MaxSelections\\", \\"TenantId\\") VALUES (2, 'Extras', false, 0, 3, '{model.TenantId}') ON CONFLICT DO NOTHING;");
            sb.AppendLine($"INSERT INTO \\"ModifierOptions\\" (\\"ProductModifierId\\", \\"Name\\", \\"PriceAdjustment\\", \\"IsDefault\\", \\"TenantId\\") VALUES (2, 'Shot Espresso', 15, false, '{model.TenantId}'), (2, 'Crema Batida', 12, false, '{model.TenantId}') ON CONFLICT DO NOTHING;");
"""

if 'Tipo de Leche' not in content:
    content = content.replace('return sb.ToString();', seed_modifiers + '\n            return sb.ToString();')

with open('./PosBuilder/SqlGenerator.cs', 'w', encoding='utf-8') as f:
    f.write(content)
