import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { ApiError, getErrorMessage } from '../../api/apiError'
import {
  PRODUCT_DESCRIPTION_MAX,
  PRODUCT_NAME_MAX,
  createProduct,
  updateProduct,
  type Product,
} from '../../api/products'
import { Button, ButtonLink } from '../../components/ui/Button'
import { Alert, Spinner } from '../../components/ui/Feedback'
import { SelectField, TextAreaField, TextField } from '../../components/ui/Field'
import { PageHeader } from '../../components/ui/PageHeader'
import { useToast } from '../../components/ui/toastContext'
import { formatCurrency } from '../../lib/format'
import { applyServerErrors, emptyToNull, optionalText, requiredText } from '../../lib/forms'
import {
  AMOUNT_FORMAT_MESSAGE,
  amountToInput,
  isValidAmountText,
  parseAmount,
} from '../../lib/money'
import { categoryKeys, useCategoryOptions } from '../categories/queries'
import { productKeys, useProduct } from './queries'

const schema = z.object({
  name: requiredText('El nombre', PRODUCT_NAME_MAX),
  description: optionalText('La descripción', PRODUCT_DESCRIPTION_MAX),
  categoryId: z.string().min(1, 'La categoría es obligatoria.'),
  // RN-08 y RF-PRO-01: ≥ 0 (se acepta 0) y 2 decimales como máximo.
  price: z
    .string()
    .trim()
    .min(1, 'El precio es obligatorio.')
    .refine(isValidAmountText, AMOUNT_FORMAT_MESSAGE),
})

type ProductForm = z.infer<typeof schema>

const FIELDS = ['name', 'description', 'categoryId', 'price'] as const

/** Alta (RF-PRO-01) y edición, incluido el precio (RF-PRO-03). Solo Administrador. */
export function ProductFormPage() {
  const { id } = useParams()
  return id ? <EditProduct id={id} /> : <ProductFormView />
}

function EditProduct({ id }: { id: string }) {
  const { data, error, isPending } = useProduct(id)

  if (isPending) {
    return <Spinner />
  }
  if (error) {
    return <Alert tone="danger">{getErrorMessage(error)}</Alert>
  }
  return <ProductFormView product={data} />
}

function ProductFormView({ product }: { product?: Product }) {
  const isEdit = product !== undefined
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [formError, setFormError] = useState<string | null>(null)
  // Solo se pueden asignar categorías activas (RN-29).
  const categories = useCategoryOptions({ onlyActive: true })

  const form = useForm<ProductForm>({
    resolver: zodResolver(schema),
    mode: 'onChange',
    defaultValues: {
      name: product?.name ?? '',
      description: product?.description ?? '',
      // Si la categoría actual está inactiva, hay que elegir otra para poder guardar.
      categoryId: product?.categoryIsActive ? product.categoryId : '',
      price: product ? amountToInput(product.price) : '',
    },
  })
  const { errors, isDirty } = form.formState

  const priceText = useWatch({ control: form.control, name: 'price' })
  const parsedPrice = parseAmount(priceText)
  const priceChanged = isEdit && parsedPrice !== null && parsedPrice !== product.price

  const save = useMutation({
    mutationFn: async (values: ProductForm) => {
      const price = parseAmount(values.price)
      if (price === null) {
        throw new Error('Precio no válido.')
      }
      const input = {
        name: values.name,
        description: emptyToNull(values.description),
        categoryId: values.categoryId,
        price,
      }
      if (isEdit) {
        await updateProduct(product.id, input)
      } else {
        await createProduct(input)
      }
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: productKeys.all }),
        // Cambia la cantidad de productos por categoría.
        queryClient.invalidateQueries({ queryKey: categoryKeys.all }),
      ])
      notify(isEdit ? 'Producto actualizado.' : 'Producto creado.')
      navigate('/products')
    },
    onError: (error) => {
      if (error instanceof ApiError && error.status === 409) {
        form.setError('name', { type: 'server', message: error.message })
        setFormError(null)
        return
      }
      setFormError(applyServerErrors(error, form.setError, FIELDS))
    },
  })

  return (
    <>
      <PageHeader
        title={isEdit ? `Editar «${product.name}»` : 'Nuevo producto'}
        back={{ to: '/products', label: 'Productos' }}
      />

      <div className="card form-card">
        {formError && <Alert tone="danger">{formError}</Alert>}
        {isEdit && !product.categoryIsActive && (
          <Alert tone="warning">
            La categoría actual, «{product.categoryName}», está inactiva. Elija una categoría activa
            para guardar los cambios.
          </Alert>
        )}
        {categories.error && <Alert tone="danger">{getErrorMessage(categories.error)}</Alert>}

        <form
          className="form"
          noValidate
          onSubmit={form.handleSubmit((values) => {
            setFormError(null)
            save.mutate(values)
          })}
        >
          <div className="form-grid">
            <div className="form-grid__wide">
              <TextField
                label="Nombre"
                required
                autoFocus
                maxLength={PRODUCT_NAME_MAX}
                error={errors.name?.message}
                hint="Único en el catálogo, sin distinguir mayúsculas."
                {...form.register('name')}
              />
            </div>
            <SelectField
              label="Categoría"
              required
              error={errors.categoryId?.message}
              disabled={categories.isPending}
              {...form.register('categoryId')}
            >
              <option value="">
                {categories.isPending ? 'Cargando categorías…' : 'Seleccione una categoría'}
              </option>
              {categories.data?.items.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </SelectField>
            <TextField
              label="Precio (₡)"
              required
              inputMode="decimal"
              autoComplete="off"
              error={errors.price?.message}
              hint={
                parsedPrice !== null
                  ? `${formatCurrency(parsedPrice)}${
                      priceChanged
                        ? ` · antes ${formatCurrency(product.price)}. Afecta a los borradores, no a las cotizaciones emitidas.`
                        : ''
                    }`
                  : 'Mayor o igual que 0, con 2 decimales como máximo.'
              }
              {...form.register('price')}
            />
            <div className="form-grid__wide">
              <TextAreaField
                label="Descripción"
                maxLength={PRODUCT_DESCRIPTION_MAX}
                error={errors.description?.message}
                hint={`Opcional. Máximo ${PRODUCT_DESCRIPTION_MAX} caracteres.`}
                {...form.register('description')}
              />
            </div>
          </div>
          <div className="form-actions">
            <ButtonLink to="/products">Cancelar</ButtonLink>
            <Button
              type="submit"
              variant="primary"
              isLoading={save.isPending}
              disabled={isEdit && !isDirty}
            >
              {isEdit ? 'Guardar cambios' : 'Crear producto'}
            </Button>
          </div>
        </form>
      </div>
    </>
  )
}
