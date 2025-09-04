import { useState, useEffect } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import WorkflowList from './components/WorkflowList'
import WorkflowForm from './components/WorkflowForm'
import ProtectedRoute from './components/ProtectedRoute'
import AdminRoute from './components/AdminRoute'
import UserRoute from './components/UserRoute'
import NavigationHeader from './components/NavigationHeader'
import UserProfile from './components/UserProfile'
import ChangePassword from './components/ChangePassword'
import UserPreferences from './components/UserPreferences'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import AdminPage from './pages/AdminPage'
import { useAuth } from './contexts/AuthContext'
import n8nApi from './services/n8nApi'

import { N8nWorkflow } from './services/n8nApi'

export interface Workflow {
  id: string
  name: string
  description?: string
  status: 'active' | 'inactive' | 'draft'
  createdAt: Date
  updatedAt: Date
  // n8n-specific fields
  active?: boolean
  nodes?: any[]
  connections?: any
  staticData?: any
  settings?: any
  tags?: string[]
}

// Helper function to convert n8n workflow to app workflow
export const convertN8nWorkflow = (n8nWorkflow: N8nWorkflow): Workflow => {
  return {
    id: n8nWorkflow.id,
    name: n8nWorkflow.name,
    description: n8nWorkflow.settings?.description || '',
    status: n8nWorkflow.active ? 'active' : 'inactive',
    createdAt: new Date(n8nWorkflow.createdAt),
    updatedAt: new Date(n8nWorkflow.updatedAt),
    active: n8nWorkflow.active,
    nodes: n8nWorkflow.nodes,
    connections: n8nWorkflow.connections,
    staticData: n8nWorkflow.staticData,
    settings: n8nWorkflow.settings,
    tags: n8nWorkflow.tags,
  }
}

// Helper function to convert app workflow to n8n workflow
export const convertToN8nWorkflow = (workflow: Omit<Workflow, 'id' | 'createdAt' | 'updatedAt'>): Partial<N8nWorkflow> => {
  return {
    name: workflow.name,
    active: workflow.status === 'active',
    settings: {
      ...workflow.settings,
      description: workflow.description,
    },
    nodes: workflow.nodes || [],
    connections: workflow.connections || {},
    staticData: workflow.staticData || {},
    tags: workflow.tags,
  }
}

function Dashboard() {
  const { user, logout } = useAuth()
  const [workflows, setWorkflows] = useState<Workflow[]>([])
  const [editingWorkflow, setEditingWorkflow] = useState<Workflow | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [executingWorkflows, setExecutingWorkflows] = useState<Set<string>>(new Set())

  // Load workflows from n8n on component mount
  useEffect(() => {
    loadWorkflows()
  }, [])

  const loadWorkflows = async () => {
    try {
      setLoading(true)
      setError(null)
      const n8nWorkflows = await n8nApi.getWorkflows()
      const convertedWorkflows = n8nWorkflows.map(convertN8nWorkflow)
      setWorkflows(convertedWorkflows)
    } catch (err) {
      console.error('Failed to load workflows:', err)
      setError('Failed to load workflows from n8n. Please check your connection.')
    } finally {
      setLoading(false)
    }
  }

  const handleAddWorkflow = () => {
    setEditingWorkflow(null)
    setShowForm(true)
  }

  const handleEditWorkflow = (workflow: Workflow) => {
    setEditingWorkflow(workflow)
    setShowForm(true)
  }

  const handleSaveWorkflow = async (workflowData: Omit<Workflow, 'id' | 'createdAt' | 'updatedAt'>) => {
    try {
      setError(null)
      const n8nWorkflowData = convertToN8nWorkflow(workflowData)

      if (editingWorkflow) {
        // Edit existing
        const updatedN8nWorkflow = await n8nApi.updateWorkflow(editingWorkflow.id, n8nWorkflowData)
        const updatedWorkflow = convertN8nWorkflow(updatedN8nWorkflow)
        setWorkflows(prev => prev.map(w =>
          w.id === editingWorkflow.id ? updatedWorkflow : w
        ))
      } else {
        // Add new
        const newN8nWorkflow = await n8nApi.createWorkflow(n8nWorkflowData)
        const newWorkflow = convertN8nWorkflow(newN8nWorkflow)
        setWorkflows(prev => [...prev, newWorkflow])
      }
      setShowForm(false)
      setEditingWorkflow(null)
    } catch (err) {
      console.error('Failed to save workflow:', err)
      setError('Failed to save workflow. Please try again.')
    }
  }

  const handleDeleteWorkflow = async (id: string) => {
    try {
      setError(null)
      await n8nApi.deleteWorkflow(id)
      setWorkflows(prev => prev.filter(w => w.id !== id))
    } catch (err) {
      console.error('Failed to delete workflow:', err)
      setError('Failed to delete workflow. Please try again.')
    }
  }

  const handleExecuteWorkflow = async (id: string) => {
    try {
      setError(null)
      setExecutingWorkflows(prev => new Set(prev).add(id))
      await n8nApi.executeWorkflow(id)
      // Optionally refresh workflows to get updated status
      await loadWorkflows()
    } catch (err) {
      console.error('Failed to execute workflow:', err)
      setError('Failed to execute workflow. Please try again.')
    } finally {
      setExecutingWorkflows(prev => {
        const newSet = new Set(prev)
        newSet.delete(id)
        return newSet
      })
    }
  }

  const handleToggleWorkflowStatus = async (id: string, currentStatus: Workflow['status']) => {
    try {
      setError(null)
      if (currentStatus === 'active') {
        await n8nApi.deactivateWorkflow(id)
      } else {
        await n8nApi.activateWorkflow(id)
      }
      // Refresh workflows to get updated status
      await loadWorkflows()
    } catch (err) {
      console.error('Failed to toggle workflow status:', err)
      setError('Failed to update workflow status. Please try again.')
    }
  }

  const handleDesignWorkflow = (id: string) => {
    try {
      setError(null)
      n8nApi.openWorkflowInN8n(id)
    } catch (err) {
      console.error('Failed to open workflow in n8n:', err)
      setError('Failed to open workflow in n8n editor.')
    }
  }

  const handleCancelForm = () => {
    setShowForm(false)
    setEditingWorkflow(null)
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading workflows from n8n...</p>
        </div>
      </div>
    )
  }

  return (
    <>
      <NavigationHeader />
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-gray-100">
        <div className="container mx-auto px-4 py-6 sm:py-8 max-w-7xl">
        <div className="mb-8 sm:mb-12">
          <div className="text-center sm:text-left">
            <div className="inline-flex items-center justify-center w-12 h-12 sm:w-16 sm:h-16 bg-gradient-to-r from-blue-600 to-purple-600 rounded-2xl mb-4 sm:mb-6">
              <svg className="w-6 h-6 sm:w-8 sm:h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <h1 className="text-2xl sm:text-4xl font-bold text-gray-900 mb-2 sm:mb-3">n8n Workflow Worklist</h1>
            <p className="text-base sm:text-lg text-gray-600 max-w-2xl mx-auto sm:mx-0 px-4 sm:px-0">Manage your n8n workflows efficiently with our professional workflow management platform</p>
          </div>
          {error && (
            <div className="p-4 bg-red-50 border border-red-200 rounded-lg max-w-2xl mx-auto sm:mx-0 mt-4">
              <p className="text-red-800 text-sm">{error}</p>
            </div>
          )}
        </div>

        <main>
          {!showForm ? (
            <div className="space-y-6 sm:space-y-8">
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div>
                  <h2 className="text-xl sm:text-2xl font-semibold text-gray-900">Your Workflows</h2>
                  <p className="text-gray-600 mt-1 text-sm sm:text-base">Create, edit, and manage your automation workflows</p>
                </div>
                <button
                  onClick={handleAddWorkflow}
                  data-add-workflow
                  className="btn-primary inline-flex items-center gap-2 self-start sm:self-auto w-full sm:w-auto justify-center"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  <span className="hidden xs:inline">Add New Workflow</span>
                  <span className="xs:hidden">Add Workflow</span>
                </button>
              </div>
              <WorkflowList
                workflows={workflows}
                onEdit={handleEditWorkflow}
                onDelete={handleDeleteWorkflow}
                onExecute={handleExecuteWorkflow}
                onToggleStatus={handleToggleWorkflowStatus}
                onDesign={handleDesignWorkflow}
                executingWorkflows={executingWorkflows}
              />
            </div>
          ) : (
            <div className="max-w-4xl mx-auto">
              <WorkflowForm
                workflow={editingWorkflow}
                onSave={handleSaveWorkflow}
                onCancel={handleCancelForm}
              />
            </div>
          )}
        </main>
        </div>
      </div>
    </>
  )
}

function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/admin"
        element={
          <AdminRoute>
            <AdminPage />
          </AdminRoute>
        }
      />
      <Route
        path="/profile"
        element={
          <UserRoute>
            <div className="min-h-screen bg-gradient-to-br from-slate-50 to-gray-100">
              <NavigationHeader />
              <div className="container mx-auto px-4 py-6 sm:py-8 max-w-7xl">
                <div className="space-y-6">
                  <UserProfile />
                  <ChangePassword />
                  <UserPreferences />
                </div>
              </div>
            </div>
          </UserRoute>
        }
      />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App