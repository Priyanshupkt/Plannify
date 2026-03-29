# Admin Master Data Page Conversion Template

## Use this template to convert remaining Master Data pages
Replace the placeholder values in brackets [LikeThis]

```html
@page "/Admin/[ENTITY_PLURAL]"
@model Plannify.Pages.Admin.[ENTITY_PLURAL].IndexModel
@{
    ViewData["Title"] = "[Entity Plural]";
    ViewData["Section"] = "Master Data";
    Layout = "/Pages/Shared/_Layout.cshtml";
}

@section TopBarAction {
    <button type="button" onclick="document.getElementById('addForm').classList.toggle('hidden')"
        class="bg-primary hover:bg-primary-container text-white px-4 py-2 rounded-xl flex items-center gap-2 text-sm font-bold shadow-lg shadow-primary/20 transition-all active:scale-95">
        <span class="material-symbols-outlined text-sm">add</span>
        Add [Entity]
    </button>
}

<!-- Add Form (Hidden by default) -->
<div id="addForm" class="hidden bg-surface-container-lowest rounded-2xl p-6 border border-outline-variant/20 mb-6">
    <h3 class="font-brand font-bold text-lg text-on-surface mb-4">New [Entity]</h3>
    <form method="post" asp-page-handler="Add" class="space-y-4">
        @Html.AntiForgeryToken()
        
        @if (!ViewData.ModelState.IsValid)
        {
            <div class="bg-error-container/20 border-l-4 border-error rounded-xl px-5 py-4 mb-4">
                <div asp-validation-summary="All" class="text-error text-sm space-y-1"></div>
            </div>
        }

        <!-- Form fields grid - customize columns as needed -->
        <div class="grid grid-cols-2 gap-4">
            <!-- COPY THIS PATTERN FOR EACH FIELD: -->
            <div>
                <label for="NewEntity_Field" class="block text-sm font-semibold text-on-surface mb-2">Field Label</label>
                <input type="text" class="w-full px-4 py-3 bg-surface-container-low border-0 rounded-xl text-sm focus:ring-2 focus:ring-primary/30 outline-none"
                    id="NewEntity_Field" asp-for="NewEntity.Field" placeholder="placeholder text" required>
                <span asp-validation-for="NewEntity.Field" class="text-error text-xs mt-1 block"></span>
            </div>
        </div>

        <div class="flex gap-3 pt-2">
            <button type="submit" class="px-6 py-3 bg-primary hover:bg-primary-container text-white font-bold rounded-xl transition-colors">
                <span class="material-symbols-outlined text-lg align-middle mr-1">check</span>
                Add [Entity]
            </button>
            <button type="button" onclick="document.getElementById('addForm').classList.add('hidden')" 
                class="px-6 py-3 bg-surface-container-low text-on-surface font-bold rounded-xl hover:bg-surface-container-high transition-colors">
                Cancel
            </button>
        </div>
    </form>
</div>

<!-- [Entity] Table -->
<div class="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 overflow-hidden shadow-sm">
    @if (Model.[ENTITY_PLURAL_PROPERTY].Count > 0)
    {
        <table class="w-full">
            <thead>
                <tr class="bg-surface-container-low/50">
                    <th class="px-8 py-4 text-[10px] font-bold text-slate-500 uppercase tracking-widest text-left">ID</th>
                    <!-- COLUMN HEADERS - customize as needed -->
                    <th class="px-4 py-4 text-[10px] font-bold text-slate-500 uppercase tracking-widest text-left">Name</th>
                    <th class="px-4 py-4 text-[10px] font-bold text-slate-500 uppercase tracking-widest text-left">Code</th>
                    <th class="px-8 py-4 text-[10px] font-bold text-slate-500 uppercase tracking-widest text-right">Actions</th>
                </tr>
            </thead>
            <tbody>
                @{
                    int index = 1;
                    foreach (var item in Model.[ENTITY_PLURAL_PROPERTY])
                    {
                        <tr class="hover:bg-surface-container-low/30 transition-colors group border-t border-outline-variant/10">
                            <td class="px-8 py-5 mono-text text-xs text-slate-400">@(index.ToString("D2"))</td>
                            <!-- DATA CELLS - customize as needed -->
                            <td class="px-4 py-5 font-bold text-on-surface">@item.Name</td>
                            <td class="px-4 py-5 mono-text text-sm text-primary">@item.Code</td>
                            <td class="px-8 py-5 text-right">
                                <div class="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                    <button type="button" onclick="editEntity(@item.Id, '@item.Name', '@item.Code')" 
                                        class="p-2 hover:bg-primary/10 text-primary rounded-lg transition-colors">
                                        <span class="material-symbols-outlined text-xl">edit</span>
                                    </button>
                                    <form method="post" asp-page-handler="Delete" asp-route-id="@item.Id" style="display:inline">
                                        @Html.AntiForgeryToken()
                                        <button type="submit" class="p-2 hover:bg-error/10 text-error rounded-lg transition-colors"
                                            onclick="return confirm('Delete @item.Name? This action cannot be undone.');">
                                            <span class="material-symbols-outlined text-xl">delete</span>
                                        </button>
                                    </form>
                                </div>
                            </td>
                        </tr>
                        index++;
                    }
                }
            </tbody>
        </table>
        <div class="px-8 py-4 bg-surface-container-low/50 flex justify-between items-center">
            <p class="text-xs text-slate-500 font-medium italic">Showing @Model.[ENTITY_PLURAL_PROPERTY].Count of @Model.[ENTITY_PLURAL_PROPERTY].Count records</p>
        </div>
    }
    else
    {
        <div class="px-8 py-16 text-center">
            <span class="material-symbols-outlined text-4xl text-slate-300 block mb-3">inbox</span>
            <p class="text-slate-400 font-medium">No [ENTITY_PLURAL_LOWERCASE] found. Create one to get started.</p>
        </div>
    }
</div>

<!-- Edit Modal -->
<div id="editModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-on-surface/40 backdrop-blur-sm hidden">
    <div class="bg-surface-container-lowest w-full max-w-md rounded-2xl shadow-2xl p-8 border border-outline-variant/20">
        <h3 class="font-brand font-bold text-xl text-on-surface mb-6">Edit [Entity]</h3>
        <form method="post" asp-page-handler="Update" class="space-y-4">
            @Html.AntiForgeryToken()
            <input type="hidden" id="editId" name="id">
            
            <!-- EDIT FORM FIELDS - customize as needed -->
            <div>
                <label class="block text-sm font-semibold text-on-surface mb-2">Name</label>
                <input type="text" id="editName" name="name" 
                    class="w-full px-4 py-3 bg-surface-container-low border-0 rounded-xl text-sm focus:ring-2 focus:ring-primary/30 outline-none" required>
            </div>
            
            <div>
                <label class="block text-sm font-semibold text-on-surface mb-2">Code</label>
                <input type="text" id="editCode" name="code" 
                    class="w-full px-4 py-3 bg-surface-container-low border-0 rounded-xl text-sm focus:ring-2 focus:ring-primary/30 outline-none" required>
            </div>

            <div class="grid grid-cols-2 gap-4 pt-4">
                <button type="button" onclick="closeEditModal()" 
                    class="px-6 py-3 bg-surface-container-low text-on-surface font-bold rounded-xl hover:bg-surface-container-high transition-colors">
                    Cancel
                </button>
                <button type="submit" class="px-6 py-3 bg-primary text-white font-bold rounded-xl hover:bg-primary-container transition-colors">
                    Save Changes
                </button>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    <script>
        function editEntity(id, name, code) {
            document.getElementById('editId').value = id;
            document.getElementById('editName').value = name;
            document.getElementById('editCode').value = code;
            document.getElementById('editModal').classList.remove('hidden');
        }
        
        function closeEditModal() {
            document.getElementById('editModal').classList.add('hidden');
        }
    </script>
}
```

## Customization Guide

### For each page, replace:
- `[ENTITY_PLURAL]` → e.g., Teachers, Subjects, Classes, Rooms
- `[Entity]` → Singular form, e.g., Teacher, Subject,Class, Room
- `[ENTITY_PLURAL_PROPERTY]` → Model property name, e.g., Teachers, Subjects
- `[ENTITY_PLURAL_LOWERCASE]` → lowercase plural, e.g., teachers, subjects
- **Form fields** - Add/remove grid-cols based on your needs:
  - 2 columns (default): `grid-cols-2`
  - 3 columns: `grid-cols-3`
  - Full width: Remove grid, use single `<div>`

### Data display customization:
- **ID column**: `mono-text text-xs text-slate-400` for monospace ID
- **Name column**: `font-bold text-on-surface` for bold primary text
- **Code column**: `mono-text text-sm text-primary` for monospace code
- **Count badge**: `px-3 py-1 bg-primary-fixed text-on-primary-fixed text-xs font-bold rounded-full` for count badges
- For dates: `text-sm text-slate-600 mono-text`

## Pages to Apply This To:
1. `/Pages/Admin/Teachers/Index.cshtml`
2. `/Pages/Admin/Subjects/Index.cshtml`
3. `/Pages/Admin/Classes/Index.cshtml`
4. `/Pages/Admin/Rooms/Index.cshtml`
5. `/Pages/Admin/AcademicYears/Index.cshtml`

## Additional Notes:
- All tables use `group` class for row-level hover state
- Action buttons reveal on hover with `opacity-0 group-hover:opacity-100`
- Empty states show inbox icon
- Record count shown at table footer

---

**After applying this template, ensure:**
1. Page directives are correct (@page, @model)
2. Layout is set to `/Pages/Shared/_Layout.cshtml`
3. Form fields match your model properties (asp-for binding)
4. Modal edit function parameters match your model fields
5. Table data cells display correct model properties
