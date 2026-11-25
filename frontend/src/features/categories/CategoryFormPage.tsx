import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { ApiError, getErrorMessage } from '../../api/apiError'
import {
  CATEGORY_DESCRIPTION_MAX,
  CATEGORY_NAME_MAX,
  createCategory,
  reactivateCategory,
  updateCategory,
  type Category,
} from '../../api/categories'
import { Button, ButtonLink } from '../../components/ui/Button'
import { Alert, Spinner } from '../../components/ui/Feedback'
import { TextAreaField, TextField } from '../../components/ui/Field'
import { PageHeader } from '../../components/ui/PageHeader'
import { useToast } from '../../components/ui/toastContext'
import { applyServerErrors, emptyToNull, optionalText, requiredText } from '../../lib/forms'
import { useDebouncedValue } from '../../lib/useDebouncedValue'
import { categoryKeys, useCategory, useInactiveCategoryNamed } from './queries'

const schema = z.object({
  name: requiredText('El nombre', CATEGORY_NAME_MAX),
  description: optionalText('La descripción', CATEGORY_DESCRIPTION_MAX),
})

type CategoryForm = z.infer<typeof schema>

const FIELDS = ['name', 'description'] as const

/** Alta (RF-CAT-01) y edición (RF-CAT-03) de una categoría. Solo Administrador. */
export function CategoryFormPage() {
  const { id } = useParams()

  if (!id) {
    return <CategoryFormView />
  }

  return <EditCategory id={id} />
}

function EditCategory({ id }: { id: string }) {
  const { data, error, isPending } = useCategory(id)

  if (isPending) {
    return <Spinner />
  }
  if (error) {
    return <Alert tone="danger">{getErrorMessage(error)}</Alert>
  }
  return <CategoryFormView category={data} />
}

function CategoryFormView({ category }: { category?: Category }) {
  const isEdit = category !== undefined
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [formError, setFormError] = useState<string | null>(null)

  const form = useForm<CategoryForm>({
    resolver: zodResolver(schema),
    mode: 'onChange',
    defaultValues: { name: category?.name ?? '', description: category?.description ?? '' },
  })
  const { errors, isDirty } = form.formState

  // Sugerencia de reactivar una categoría inactiva con el mismo nombre (RF-CAT-01).
  const name = useWatch({ control: form.control, name: 'name' })
  const debouncedName = useDebouncedValue(isEdit ? '' : name)
  const { data: inactiveMatch } = useInactiveCategoryNamed(debouncedName)

  const finish = async (message: string) => {
    await queryClient.invalidateQueries({ queryKey: categoryKeys.all })
    notify(message)
    navigate('/categories')
  }

  const save = useMutation({
    mutationFn: async (values: CategoryForm) => {
      const input = { name: values.name, description: emptyToNull(values.description) }
      if (isEdit) {
        await updateCategory(category.id, input)
      } else {
        await createCategory(input)
      }
    },
    onSuccess: () => finish(isEdit ? 'Categoría actualizada.' : 'Categoría creada.'),
    onError: (error) => {
      // Un nombre repetido (409) se muestra junto al campo nombre.
      if (error instanceof ApiError && error.status === 409) {
        form.setError('name', { type: 'server', message: error.message })
        setFormError(null)
        return
      }
      setFormError(applyServerErrors(error, form.setError, FIELDS))
    },
  })

  const reactivate = useMutation({
    mutationFn: (match: Category) => reactivateCategory(match.id),
    onSuccess: (_, match) => finish(`Se reactivó la categoría «${match.name}».`),
  })

  return (
    <>
      <PageHeader
        title={isEdit ? `Editar «${category.name}»` : 'Nueva categoría'}
        back={{ to: '/categories', label: 'Categorías' }}
      />

      <div className="card form-card">
        {formError && <Alert tone="danger">{formError}</Alert>}

        {inactiveMatch && (
          <Alert
            tone="warning"
            title="Ya existe una categoría inactiva con este nombre"
            actions={
              <Button
                variant="primary"
                isLoading={reactivate.isPending}
                onClick={() => reactivate.mutate(inactiveMatch)}
              >
                Reactivar «{inactiveMatch.name}»
              </Button>
            }
          >
            Puede reactivarla en lugar de crear una nueva; conserva sus productos.
            {reactivate.error && <p>{getErrorMessage(reactivate.error)}</p>}
          </Alert>
        )}

        <form
          className="form"
          noValidate
          onSubmit={form.handleSubmit((values) => {
            setFormError(null)
            save.mutate(values)
          })}
        >
          <TextField
            label="Nombre"
            required
            autoFocus
            maxLength={CATEGORY_NAME_MAX}
            error={errors.name?.message}
            hint={`Único, sin distinguir mayúsculas. Máximo ${CATEGORY_NAME_MAX} caracteres.`}
            {...form.register('name')}
          />
          <TextAreaField
            label="Descripción"
            maxLength={CATEGORY_DESCRIPTION_MAX}
            error={errors.description?.message}
            hint={`Opcional. Máximo ${CATEGORY_DESCRIPTION_MAX} caracteres.`}
            {...form.register('description')}
          />
          {isEdit && (
            <p className="muted">
              Las cotizaciones emitidas conservan el nombre de categoría con el que se emitieron.
            </p>
          )}
          <div className="form-actions">
            <ButtonLink to="/categories">Cancelar</ButtonLink>
            <Button
              type="submit"
              variant="primary"
              isLoading={save.isPending}
              disabled={isEdit && !isDirty}
            >
              {isEdit ? 'Guardar cambios' : 'Crear categoría'}
            </Button>
          </div>
        </form>
      </div>
    </>
  )
}
