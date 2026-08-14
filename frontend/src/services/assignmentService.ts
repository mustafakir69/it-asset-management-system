import { mockAssignments } from '../mocks/assignments'
import type { Asset } from '../types/asset'
import type {
  Assignment,
  CreateAssignmentInput,
  ReturnAssignmentInput,
} from '../types/assignment'
import { assetService } from './assetService'

export interface AssignmentService {
  getActiveAssignments: () => Promise<Assignment[]>
  getAssignmentHistory: () => Promise<Assignment[]>
  getAssignmentById: (id: string) => Promise<Assignment | undefined>
  getAssignableAssets: () => Promise<Asset[]>
  createAssignment: (input: CreateAssignmentInput) => Promise<Assignment>
  returnAssignment: (id: string, input: ReturnAssignmentInput) => Promise<Assignment>
}

let assignmentStore: Assignment[] = mockAssignments.map((assignment) => ({ ...assignment }))
let nextAssignmentSequence = assignmentStore.length + 1
const pendingAssetIds = new Set<string>()
const pendingReturnIds = new Set<string>()

const cloneAssignment = (assignment: Assignment): Assignment => ({ ...assignment })
const hasActiveAssignment = (assetId: string) =>
  assignmentStore.some(
    (assignment) => assignment.assetId === assetId && assignment.returnedAt === null,
  )

const validateRequiredText = (value: string, fieldName: string) => {
  if (!value.trim()) {
    throw new Error(`${fieldName} alanı zorunludur.`)
  }
}

export const assignmentService: AssignmentService = {
  getActiveAssignments: () =>
    Promise.resolve(
      assignmentStore
        .filter((assignment) => assignment.returnedAt === null)
        .map(cloneAssignment),
    ),

  getAssignmentHistory: () =>
    Promise.resolve(
      [...assignmentStore]
        .sort((first, second) => second.assignedAt.localeCompare(first.assignedAt))
        .map(cloneAssignment),
    ),

  getAssignmentById: (id) => {
    const assignment = assignmentStore.find((current) => current.id === id)
    return Promise.resolve(assignment ? cloneAssignment(assignment) : undefined)
  },

  getAssignableAssets: async () => {
    const assets = await assetService.getAssets()

    return assets.filter(
      (asset) =>
        asset.status === 'Stokta' &&
        !hasActiveAssignment(asset.id) &&
        !pendingAssetIds.has(asset.id),
    )
  },

  createAssignment: async (input) => {
    validateRequiredText(input.assetId, 'Cihaz')
    validateRequiredText(input.employeeName, 'Çalışan')
    validateRequiredText(input.department, 'Departman')
    validateRequiredText(input.assignedAt, 'Zimmet tarihi')
    validateRequiredText(input.assignedBy, 'Zimmetleyen')

    if (hasActiveAssignment(input.assetId) || pendingAssetIds.has(input.assetId)) {
      throw new Error('Bu cihazın zaten aktif bir zimmeti bulunuyor.')
    }

    pendingAssetIds.add(input.assetId)

    try {
      const asset = await assetService.getAssetById(input.assetId)

      if (!asset) {
        throw new Error('Zimmetlenecek cihaz bulunamadı.')
      }

      if (asset.status !== 'Stokta') {
        throw new Error('Yalnızca stokta bulunan cihazlar zimmetlenebilir.')
      }

      await assetService.updateAssetStatus(asset.id, 'Zimmetli')

      const createdAssignment: Assignment = {
        id: `assignment-${String(nextAssignmentSequence).padStart(3, '0')}`,
        assetId: asset.id,
        assetCode: asset.assetCode,
        assetBrand: asset.brand,
        assetModel: asset.model,
        employeeId: `employee-${String(nextAssignmentSequence).padStart(3, '0')}`,
        employeeName: input.employeeName.trim(),
        department: input.department.trim(),
        assignedAt: input.assignedAt,
        assignedBy: input.assignedBy.trim(),
        returnedAt: null,
        returnedBy: null,
        returnNotes: null,
        notes: input.notes?.trim() || null,
      }

      nextAssignmentSequence += 1
      assignmentStore = [createdAssignment, ...assignmentStore]

      return cloneAssignment(createdAssignment)
    } finally {
      pendingAssetIds.delete(input.assetId)
    }
  },

  returnAssignment: async (id, input) => {
    validateRequiredText(input.returnedAt, 'İade tarihi')
    validateRequiredText(input.returnedBy, 'İade alan')

    const assignment = assignmentStore.find((current) => current.id === id)

    if (!assignment || assignment.returnedAt !== null) {
      throw new Error('Aktif zimmet kaydı bulunamadı.')
    }

    if (input.returnedAt < assignment.assignedAt) {
      throw new Error('İade tarihi zimmet tarihinden önce olamaz.')
    }

    if (pendingReturnIds.has(id)) {
      throw new Error('Bu zimmet için iade işlemi devam ediyor.')
    }

    pendingReturnIds.add(id)

    try {
      await assetService.updateAssetStatus(assignment.assetId, 'Stokta')

      const returnedAssignment: Assignment = {
        ...assignment,
        returnedAt: input.returnedAt,
        returnedBy: input.returnedBy.trim(),
        returnNotes: input.returnNotes?.trim() || null,
      }

      assignmentStore = assignmentStore.map((current) =>
        current.id === id ? returnedAssignment : current,
      )

      return cloneAssignment(returnedAssignment)
    } finally {
      pendingReturnIds.delete(id)
    }
  },
}
