import re

with open('./PosCore/ViewModels/MainViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# I need to add DbContext to the AddToCart logic. Or query the DB to see if product has modifiers.
# Let's see if we have access to _dbContext in MainViewModel.
