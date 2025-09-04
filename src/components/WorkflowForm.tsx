import { useState, useEffect } from 'react'
import { Workflow } from '../App'

interface WorkflowFormProps {
  workflow: Workflow | null
  onSave: (workflow: Omit<Workflow, 'id' | 'createdAt' | 'updatedAt'>) => void
  onCancel: () => void
}

const WorkflowForm = ({ workflow, onSave, onCancel }: WorkflowFormProps) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    status: 'draft' as Workflow['status']
  })

  useEffect(() => {
    if (workflow) {
      setFormData({
        name: workflow.name,
        description: workflow.description || '',
        status: workflow.status
      })
    } else {
      setFormData({
        name: '',
        description: '',
        status: 'draft'
      })
    }
  }, [workflow])

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!formData.name.trim()) {
      alert('Please enter a workflow name')
      return
    }
    onSave(formData)
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  return (
    <div className="card">
      <div className="card-header bg-gradient-to-r from-blue-50 to-purple-50 border-b-0">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {workflow ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              )}
            </svg>
          </div>
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              {workflow ? 'Edit Workflow' : 'Create New Workflow'}
            </h2>
            <p className="text-sm text-gray-600 mt-0.5">
              {workflow ? 'Update your workflow details' : 'Set up a new automation workflow'}
            </p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="p-4 sm:p-6 space-y-6 sm:space-y-8">
        <div className="space-y-5 sm:space-y-6">
          <div>
            <label htmlFor="name" className="label flex items-center gap-1">
              Workflow Name
              <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              className="input-field"
              placeholder="Enter a descriptive name for your workflow"
              required
            />
            <p className="text-xs text-gray-500 mt-1">Choose a clear, descriptive name that explains what this workflow does</p>
          </div>

          <div>
            <label htmlFor="description" className="label">
              Description
            </label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows={3}
              className="input-field resize-none"
              placeholder="Describe what this workflow accomplishes and any important details..."
            />
            <p className="text-xs text-gray-500 mt-1">Optional: Provide additional context about this workflow's purpose</p>
          </div>

          <div>
            <label htmlFor="status" className="label">
              Status
            </label>
            <select
              id="status"
              name="status"
              value={formData.status}
              onChange={handleChange}
              className="input-field"
            >
              <option value="draft">📝 Draft - Work in progress</option>
              <option value="active">✅ Active - Running and operational</option>
              <option value="inactive">⏸️ Inactive - Temporarily disabled</option>
            </select>
            <p className="text-xs text-gray-500 mt-1">Set the current status of your workflow</p>
          </div>
        </div>

        <div className="flex flex-col-reverse sm:flex-row sm:justify-end gap-3 pt-4 sm:pt-6 border-t border-gray-200">
          <button
            type="button"
            onClick={onCancel}
            className="btn-secondary w-full sm:w-auto"
          >
            Cancel
          </button>
          <button
            type="submit"
            className="btn-primary inline-flex items-center justify-center gap-2 w-full sm:w-auto"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {workflow ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              )}
            </svg>
            {workflow ? 'Update Workflow' : 'Create Workflow'}
          </button>
        </div>
      </form>
    </div>
  )
}

export default WorkflowForm